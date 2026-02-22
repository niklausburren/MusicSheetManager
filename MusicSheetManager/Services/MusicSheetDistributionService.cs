using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MusicSheetManager.Models;
using MusicSheetManager.Utilities;
using MusicSheetManager.Views;
using OfficeOpenXml;

namespace MusicSheetManager.Services
{
    public class MusicSheetDistributionService : IMusicSheetDistributionService
    {
        #region Constructors

        public MusicSheetDistributionService(IMusicSheetAssignmentService musicSheetAssignmentService, IPeopleService peopleService, IPlaylistService playlistService)
        {
            this.MusicSheetAssignmentService = musicSheetAssignmentService;
            this.PeopleService = peopleService;
            this.PlaylistService = playlistService;
        }

        #endregion


        #region Properties

        private IMusicSheetAssignmentService MusicSheetAssignmentService { get; }

        private IPeopleService PeopleService { get; }

        private IPlaylistService PlaylistService { get; }

        #endregion


        #region Private Methods

        private static void EnsureDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }
        }

        private static string ComputeSha256Hex(string file)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(file);
            var hash = sha.ComputeHash(stream);

            var sb = new StringBuilder(hash.Length * 2);

            foreach (var b in hash)
            {
                sb.Append(b.ToString("x2"));
            }
            
            return sb.ToString();
        }

        private static bool FilesDifferByHash(string sourceFile, string destinationFile)
        {
            if (!File.Exists(destinationFile))
            {
                return true;
            }

            // Quick reject to avoid hashing in common cases
            var srcInfo = new FileInfo(sourceFile);
            var dstInfo = new FileInfo(destinationFile);

            if (srcInfo.Length != dstInfo.Length)
            {
                return true;
            }

            var srcHash = ComputeSha256Hex(sourceFile);
            var dstHash = ComputeSha256Hex(destinationFile);

            return !srcHash.Equals(dstHash, StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<string> EnumerateDirectoriesBottomUp(string root)
        {
            if (!Directory.Exists(root))
            {
                yield break;
            }

            foreach (var dir in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories).OrderByDescending(d => d.Length))
            {
                yield return dir;
            }
        }

        private DistributionPlan BuildDistributionPlan()
        {
            // Compute expected state
            var expectedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var expectedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Folders.DistributionFolder };

            var copyOperations = new List<CopyOperation>(capacity: 1024);

            // Track which titles are missing per person
            var missingTitlesByPerson = new Dictionary<Person, HashSet<string>>();

            var instruments = this.PeopleService.People.Select(p => p.Instrument).Distinct().ToList();

            // Non-percussion
            foreach (var instrument in instruments.Where(i => i.Category != InstrumentCategory.Percussion))
            {
                foreach (var person in this.PeopleService.People.Where(p => p.Instrument == instrument))
                {
                    if (person.Dispensed)
                    {
                        continue;
                    }

                    var personFolder = Path.Combine(Folders.DistributionFolder, $"{instrument.Index:D2} {instrument.DisplayName}", person.FullName);
                    expectedDirectories.Add(personFolder);

                    foreach (var playlist in this.PlaylistService.Playlists.Where(p => p.Distribute))
                    {
                        var playlistFolder = Path.Combine(personFolder, playlist.SanitizedName);
                        expectedDirectories.Add(playlistFolder);

                        foreach (var entry in playlist.Entries.Where(e => e.Distribute && e.MusicSheetFolder != null))
                        {
                            var musicSheet = this.MusicSheetAssignmentService.GetAssignedMusicSheet(entry.MusicSheetFolder, person);

                            if (musicSheet != null)
                            {
                                var dest = Path.Combine(playlistFolder, $"{entry.Number} {Path.GetFileName(musicSheet.FileName)}");
                                expectedFiles.Add(dest);
                                copyOperations.Add(new CopyOperation(musicSheet.FileName, dest));
                            }
                            else
                            {
                                if (!missingTitlesByPerson.TryGetValue(person, out var titles))
                                {
                                    titles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                    missingTitlesByPerson[person] = titles;
                                }

                                // Track the title of the folder (piece) missing for this person
                                titles.Add(entry.MusicSheetFolder.Title);
                            }
                        }
                    }
                }
            }

            // Percussion
            foreach (var playlist in this.PlaylistService.Playlists.Where(p => p.Distribute))
            {
                var percussionFolder = Path.Combine(
                    Folders.DistributionFolder,
                    $"{InstrumentInfo.Percussion.Index:D2} {InstrumentInfo.Percussion.DisplayName}",
                    playlist.SanitizedName);

                expectedDirectories.Add(percussionFolder);

                foreach (var entry in playlist.Entries.Where(e => e.Distribute && e.MusicSheetFolder != null))
                {
                    var percussionSubFolder = Path.Combine(percussionFolder, $"{entry.Number} {entry.MusicSheetFolder.Title}");
                    expectedDirectories.Add(percussionSubFolder);

                    foreach (var musicSheet in entry.MusicSheetFolder.Sheets.Where(s => s.Instrument.Category == InstrumentCategory.Percussion))
                    {
                        var dest = Path.Combine(percussionSubFolder, $"{entry.Number} {Path.GetFileName(musicSheet.FileName)}");
                        expectedFiles.Add(dest);
                        copyOperations.Add(new CopyOperation(musicSheet.FileName, dest));
                    }
                }
            }

            // Plan delete files that are not expected (if distribution folder exists)
            var filesToDelete = new List<string>();

            if (Directory.Exists(Folders.DistributionFolder))
            {
                filesToDelete.AddRange(Directory.EnumerateFiles(Folders.DistributionFolder, "*", SearchOption.AllDirectories)
                    .Where(existingFile => !expectedFiles.Contains(existingFile)));
            }

            // Plan directory cleanup: delete empty dirs not expected (bottom-up)
            var dirsToDeleteIfEmpty = new List<string>();

            if (Directory.Exists(Folders.DistributionFolder))
            {
                dirsToDeleteIfEmpty.AddRange(EnumerateDirectoriesBottomUp(Folders.DistributionFolder)
                    .Where(dir => !expectedDirectories.Contains(dir)));
            }

            // Directories to create: expected dirs ordered top-down (shortest first)
            var dirsToCreate = expectedDirectories
                .OrderBy(d => d.Length)
                .ToList();

            // Make deletes deterministic
            filesToDelete.Sort(StringComparer.OrdinalIgnoreCase);

            // Build immutable MissingSheetInfo list
            var missingSheets = missingTitlesByPerson
                .OrderBy(kvp => kvp.Key.FullName, StringComparer.OrdinalIgnoreCase)
                .Select(kvp => new MissingSheetInfo(
                    kvp.Key,
                    kvp.Value.OrderBy(t => t, StringComparer.OrdinalIgnoreCase).ToArray()))
                .ToList();

            return new DistributionPlan(
                directoriesToCreate: dirsToCreate,
                filesToEnsure: copyOperations,
                filesToDelete: filesToDelete,
                directoriesToDeleteIfEmpty: dirsToDeleteIfEmpty,
                missingSheets: missingSheets);
        }

        private static int CalculateProgressPercent(int done, int total)
        {
            if (total <= 0)
            {
                return 0;
            }

            var pct = (int)Math.Round(done * 100.0 / total, MidpointRounding.AwayFromZero);
            return Math.Clamp(pct, 0, 100);
        }

        private static string CreateUniqueTempFilePath(string baseName, string extensionWithoutDot)
        {
            // Unique suffix
            var unique = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);

            var fileName = $"{baseName} {unique}.{extensionWithoutDot}";
            return Path.Combine(Folders.TempFolder, fileName);
        }

        #endregion


        #region IMusicSheetDistributionService Members

        public Task DistributeAsync(IDistributionReporter reporter, CancellationToken cancellationToken = default)
        {
            EnsureDirectory(Folders.DistributionFolder);

            reporter.SetHeader("/Resources/sync.png", "Distributing sheets...");
            reporter.ReportProgress(0, "Preparing...");

            return Task.Run(async () =>
            {
                var copied = 0;
                var deleted = 0;
                var skipped = 0;

                var warningCount = 0;
                var errorCount = 0;

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    reporter.ReportProgress(0, "Planning...");

                    var plan = this.BuildDistributionPlan();

                    // exact linear progress
                    var total =
                        plan.DirectoriesToCreate.Count +
                        plan.FilesToEnsure.Count +
                        plan.FilesToDelete.Count +
                        plan.DirectoriesToDeleteIfEmpty.Count;

                    total = Math.Max(1, total);
                    var done = 0;

                    void Step(string status)
                    {
                        done++;
                        reporter.ReportProgress(CalculateProgressPercent(done, total), status);
                    }

                    // 1) mkdir
                    foreach (var newDirectory in plan.DirectoriesToCreate)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Step($"Ensuring directory: {newDirectory}");

                        if (Directory.Exists(newDirectory))
                        {
                            continue;
                        }

                        try
                        {
                            reporter.AppendLog(DistributionLogLevel.Info, $"CREATE DIR  {newDirectory}");
                            Directory.CreateDirectory(newDirectory!);
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            errorCount++;
                            reporter.AppendLog(DistributionLogLevel.Error, $"Creating dir failed: {newDirectory} :: {ex.Message}");
                        }
                    }

                    // 2) copy/update by hash
                    foreach (var copyOperation in plan.FilesToEnsure)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Step($"Comparing: {Path.GetFileName(copyOperation.DestinationFile)}");

                        try
                        {
                            EnsureDirectory(Path.GetDirectoryName(copyOperation.DestinationFile)!);
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            reporter.AppendLog(DistributionLogLevel.Error, $"Ensuring directory failed: {Path.GetDirectoryName(copyOperation.DestinationFile)} :: {ex.Message}");
                            errorCount++;
                            continue;
                        }

                        try
                        {
                            if (FilesDifferByHash(copyOperation.SourceFile, copyOperation.DestinationFile))
                            {
                                reporter.AppendLog(DistributionLogLevel.Info, $"COPY FILE {copyOperation.DestinationFile}");
                                copied++;
                                File.Copy(copyOperation.SourceFile, copyOperation.DestinationFile!, overwrite: true);
                            }
                            else
                            {
                                skipped++;
                            }
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            reporter.AppendLog(DistributionLogLevel.Error, $"Copy file failed: {copyOperation.SourceFile} -> {copyOperation.DestinationFile} :: {ex.Message}");
                            errorCount++;
                        }
                    }

                    // 3) delete obsolete files
                    foreach (var file in plan.FilesToDelete)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Step($"Deleting: {Path.GetFileName(file)}");

                        if (!File.Exists(file))
                        {
                            continue;
                        }

                        reporter.AppendLog(DistributionLogLevel.Info, $"DELETE FILE {file}");
                        deleted++;

                        var result = await FileSystemHelper.TryDeleteFileAsync(file);

                        if (!result.Success)
                        {
                            reporter.AppendLog(DistributionLogLevel.Error, $"Delete file failed: {file} :: {result.ErrorMessage}");
                            errorCount++;
                        }
                    }

                    // 4) remove empty obsolete dirs (bottom-up)
                    foreach (var dir in plan.DirectoriesToDeleteIfEmpty)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        Step($"Cleaning directory: {dir}");

                        if (!Directory.Exists(dir))
                        {
                            continue;
                        }

                        if (Directory.EnumerateFileSystemEntries(dir).Any())
                        {
                            continue;
                        }

                        reporter.AppendLog(DistributionLogLevel.Info, $"RMDIR {dir}");

                        var result = await FileSystemHelper.TryDeleteFolderAsync(dir);

                        if (!result.Success)
                        {
                            reporter.AppendLog(DistributionLogLevel.Error, $"Remove directory failed: {dir} :: {result.ErrorMessage}");
                            errorCount++;
                        }
                    }

                    // Log warnings for missing sheets (per person, list titles)
                    foreach (var missingSheetInfo in plan.MissingSheets)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var titles = string.Join(", ", missingSheetInfo.Titles);
                        warningCount++;
                        reporter.AppendLog(
                            DistributionLogLevel.Warning,
                            $"Missing sheets for: {missingSheetInfo.Person.FullName} ({missingSheetInfo.Person.Instrument.DisplayName}), missing titles: {titles}");
                    }

                    var summary = $"Summary: copied={copied}, deleted={deleted}, skipped={skipped}, warnings={warningCount}, errors={errorCount}";

                    reporter.AppendLog(DistributionLogLevel.Info, summary);

                    // Final header state
                    if (errorCount > 0)
                    {
                        reporter.MarkCompleted("/Resources/error.png", "Sheets distributed with errors", summary);
                    }
                    else if (warningCount > 0)
                    {
                        reporter.MarkCompleted("/Resources/warning.png", "Sheets distributed with warnings", summary);
                    }
                    else
                    {
                        reporter.MarkCompleted("/Resources/success.png", "Sheets distributed successfully", summary);
                    }
                }
                catch (OperationCanceledException)
                {
                    reporter.AppendLog(DistributionLogLevel.Warning, "Operation cancelled");
                    reporter.MarkCompleted("/Resources/warning.png", "Sheets distribution cancelled", "Distribution cancelled by user");
                }
                catch (Exception ex)
                {
                    reporter.AppendLog(DistributionLogLevel.Error, "Unhandled exception:");
                    reporter.AppendLog(DistributionLogLevel.Error, ex.ToString());

                    reporter.MarkCompleted("/Resources/error.png", "Sheets distributed with errors", "Completed with errors (see log).");
                }
            }, cancellationToken);
        }

        public void ExportSheetDistribution(Playlist playlist)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Niklaus Burren");

            using var package = new ExcelPackage();

            // Worksheet mit Playlist-Namen erstellen (ungültige Zeichen entfernen)
            var sheetName = playlist.SanitizedName;
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            // Titel mit Playlist-Namen in Zeile 1
            worksheet.Cells[1, 1].Value = playlist.Name;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;

            // Header-Zeile in Zeile 3 erstellen
            var headerRow = 3;
            worksheet.Cells[headerRow, 1].Value = "Person";
            var column = 2;
            var musicSheetFolders = new List<MusicSheetFolder>();

            foreach (var entry in playlist.Entries.Where(e => e.MusicSheetFolder != null))
            {
                musicSheetFolders.Add(entry.MusicSheetFolder);
                worksheet.Cells[headerRow, column].Value = $"{entry.Index+1:D2} {entry.MusicSheetFolder.Title}";
                column++;
            }

            // Header formatieren
            using (var range = worksheet.Cells[headerRow, 1, headerRow, column - 1])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Erste Spalte fixieren (Freeze Panes)
            worksheet.View.FreezePanes(headerRow + 1, 2);

            // Personen nach Instrument gruppiert auflisten (ohne Percussion)
            var row = headerRow + 1;
            var groupedPeople = this.PeopleService.People
                .Where(p => !p.Dispensed && p.Instrument.Category != InstrumentCategory.Percussion)
                .OrderBy(p => p.Instrument.Index)
                .ThenBy(p => p.Part?.Index ?? int.MaxValue)
                .ThenBy(p => p.FullName)
                .GroupBy(p => p.Instrument);

            foreach (var instrumentGroup in groupedPeople)
            {
                // Instrument als Titel-Zeile
                worksheet.Cells[row, 1].Value = instrumentGroup.Key.DisplayName;
                using (var range = worksheet.Cells[row, 1, row, column - 1])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }
                row++;

                // Personen des Instruments auflisten
                foreach (var person in instrumentGroup)
                {
                    var partInfo = person.Part != null ? $" ({person.Part.DisplayName})" : "";
                    worksheet.Cells[row, 1].Value = $"{person.FullName}{partInfo}";

                    column = 2;
                    foreach (var musicSheetFolder in musicSheetFolders)
                    {
                        var assignedSheet = this.MusicSheetAssignmentService.GetAssignedMusicSheet(musicSheetFolder, person);

                        if (assignedSheet != null)
                        {
                            var sheetInfo = assignedSheet.Instrument.DisplayName;

                            if (assignedSheet.Parts != null && assignedSheet.Parts.Any())
                            {
                                var parts = string.Join(", ", assignedSheet.Parts.Select(p => p.DisplayName));
                                sheetInfo += $" - {parts}";
                            }

                            worksheet.Cells[row, column].Value = sheetInfo;
                        }
                        else
                        {
                            // Zelle gelb markieren, wenn kein Part zugeordnet werden konnte
                            worksheet.Cells[row, column].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            worksheet.Cells[row, column].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        }

                        column++;
                    }

                    row++;
                }
            }

            // Feinen grauen Rahmen um die gesamte Tabelle (Header + Daten)
            var lastRow = row - 1;
            var lastColumn = column - 1;

            if (lastRow >= headerRow && lastColumn >= 1)
            {
                using (var range = worksheet.Cells[headerRow, 1, lastRow, lastColumn])
                {
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);
                }

                // Innere Rahmen für alle Zellen
                for (var r = headerRow; r <= lastRow; r++)
                {
                    for (var c = 1; c <= lastColumn; c++)
                    {
                        worksheet.Cells[r, c].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        worksheet.Cells[r, c].Style.Border.Top.Color.SetColor(System.Drawing.Color.Gray);
                        worksheet.Cells[r, c].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        worksheet.Cells[r, c].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Gray);
                        worksheet.Cells[r, c].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        worksheet.Cells[r, c].Style.Border.Left.Color.SetColor(System.Drawing.Color.Gray);
                        worksheet.Cells[r, c].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        worksheet.Cells[r, c].Style.Border.Right.Color.SetColor(System.Drawing.Color.Gray);
                    }
                }

                // Spaltenbreite automatisch anpassen
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            // Datei speichern mit garantiert eindeutigem Namen
            var filePath = CreateUniqueTempFilePath($"Sheet distribution {playlist.SanitizedName}", "xlsx");
            var fileInfo = new FileInfo(filePath);
            package.SaveAs(fileInfo);

            Process.Start(new ProcessStartInfo
            {
                FileName = fileInfo.FullName,
                UseShellExecute = true
            });
        }

        #endregion


        #region Class CopyOperation

        private sealed class CopyOperation
        {
            #region Constructors

            public CopyOperation(string sourceFile, string destinationFile)
            {
                this.SourceFile = sourceFile;
                this.DestinationFile = destinationFile;
            }

            #endregion


            #region Properties

            public string SourceFile { get; }

            public string DestinationFile { get; }

            #endregion
        }

        #endregion


        #region Class DistributionPlan

        private sealed class DistributionPlan
        {
            #region Constructors

            public DistributionPlan(
                IReadOnlyList<string> directoriesToCreate,
                IReadOnlyList<CopyOperation> filesToEnsure,
                IReadOnlyList<string> filesToDelete,
                IReadOnlyList<string> directoriesToDeleteIfEmpty,
                IReadOnlyList<MissingSheetInfo> missingSheets)
            {
                this.DirectoriesToCreate = directoriesToCreate;
                this.FilesToEnsure = filesToEnsure;
                this.FilesToDelete = filesToDelete;
                this.DirectoriesToDeleteIfEmpty = directoriesToDeleteIfEmpty;
                this.MissingSheets = missingSheets;
            }

            #endregion


            #region Properties

            public IReadOnlyList<string> DirectoriesToCreate { get; }

            public IReadOnlyList<CopyOperation> FilesToEnsure { get; }

            public IReadOnlyList<string> FilesToDelete { get; }

            public IReadOnlyList<string> DirectoriesToDeleteIfEmpty { get; }

            public IReadOnlyList<MissingSheetInfo> MissingSheets { get; }

            #endregion
        }

        #endregion


        #region Class MissingSheetInfo

        private sealed class MissingSheetInfo
        {
            #region Constructors

            public MissingSheetInfo(Person person, IReadOnlyCollection<string> titles)
            {
                this.Person = person;
                this.Titles = titles;
            }

            #endregion


            #region Properties

            public Person Person { get; }

            public IReadOnlyCollection<string> Titles { get; }

            #endregion
        }

        #endregion
    }
}
