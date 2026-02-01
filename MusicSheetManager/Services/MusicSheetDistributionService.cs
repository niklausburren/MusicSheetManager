using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MusicSheetManager.Models;
using MusicSheetManager.Properties;
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

        private string SanitizeSheetName(string name)
        {
            var invalidChars = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            var sanitized = name;

            foreach (var invalidChar in invalidChars)
            {
                sanitized = sanitized.Replace(invalidChar, '_');
            }

            if (sanitized.Length > 31)
            {
                sanitized = sanitized.Substring(0, 31);
            }

            return sanitized;
        }

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

            foreach (var dir in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories)
                         .OrderByDescending(d => d.Length))
            {
                yield return dir;
            }
        }

        private DistributionPlan BuildDistributionPlan(HashSet<Person> peopleWithMissingMusicSheets)
        {
            // Compute expected state
            var expectedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var expectedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Folders.DistributionFolder
            };

            var copyOps = new List<CopyOperation>(capacity: 1024);

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
                                copyOps.Add(new CopyOperation(musicSheet.FileName, dest));
                            }
                            else
                            {
                                peopleWithMissingMusicSheets.Add(person);
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
                        copyOps.Add(new CopyOperation(musicSheet.FileName, dest));
                    }
                }
            }

            // Plan delete files that are not expected (if distribution folder exists)
            var filesToDelete = new List<string>();
            if (Directory.Exists(Folders.DistributionFolder))
            {
                foreach (var existingFile in Directory.EnumerateFiles(Folders.DistributionFolder, "*", SearchOption.AllDirectories))
                {
                    if (!expectedFiles.Contains(existingFile))
                    {
                        filesToDelete.Add(existingFile);
                    }
                }
            }

            // Plan directory cleanup: delete empty dirs not expected (bottom-up)
            var dirsToDeleteIfEmpty = new List<string>();
            if (Directory.Exists(Folders.DistributionFolder))
            {
                foreach (var dir in EnumerateDirectoriesBottomUp(Folders.DistributionFolder))
                {
                    if (!expectedDirectories.Contains(dir))
                    {
                        dirsToDeleteIfEmpty.Add(dir);
                    }
                }
            }

            // Directories to create: expected dirs ordered top-down (shortest first)
            var dirsToCreate = expectedDirectories
                .OrderBy(d => d.Length)
                .ToList();

            // Make deletes deterministic
            filesToDelete.Sort(StringComparer.OrdinalIgnoreCase);

            return new DistributionPlan(
                DirectoriesToCreate: dirsToCreate,
                FilesToEnsure: copyOps,
                FilesToDelete: filesToDelete,
                DirectoriesToDeleteIfEmpty: dirsToDeleteIfEmpty);
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

        #endregion


        #region IMusicSheetDistributionService Members

        public void Distribute()
        {
            EnsureDirectory(Folders.DistributionFolder);

            var owner = Application.Current?.MainWindow;

            var dialog = new DistributionDialog
            {
                Owner = owner,
                ViewModel =
                {
                    CanClose = false
                }
            };

            // initial header/status
            dialog.SetHeader("/Resources/sync.png", "Distribute sheets...");
            dialog.ReportProgress(0, "Preparing...");

            Task.Run(() =>
            {
                var copied = 0;
                var deleted = 0;
                var skipped = 0;

                var warningCount = 0;
                var errorCount = 0;

                try
                {
                    dialog.ReportProgress(0, "Planning...");
                    var peopleWithMissingMusicSheets = new HashSet<Person>();
                    var plan = this.BuildDistributionPlan(peopleWithMissingMusicSheets);

                    // Log warnings for missing sheets (per person, once)
                    foreach (var person in peopleWithMissingMusicSheets.OrderBy(p => p.FullName))
                    {
                        warningCount++;
                        dialog.AppendLogLine(DistributionLogLevel.Warning, $"Missing sheet assignment for: {person.FullName} ({person.Instrument.DisplayName})");
                    }

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
                        dialog.ReportProgress(CalculateProgressPercent(done, total), status);
                    }

                    // 1) mkdir
                    foreach (var dir in plan.DirectoriesToCreate)
                    {
                        Step($"Ensuring directory: {dir}");

                        if (Directory.Exists(dir))
                        {
                            continue;
                        }

                        try
                        {
                            Directory.CreateDirectory(dir);
                            dialog.AppendLogLine(DistributionLogLevel.Info, $"CREATE DIR  {dir}");
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            errorCount++;
                            dialog.AppendLogLine(DistributionLogLevel.Error, $"Creating directory failed: {dir} :: {ex.Message}");
                        }
                    }

                    // 2) copy/update by hash
                    foreach (var op in plan.FilesToEnsure)
                    {
                        Step($"Comparing: {Path.GetFileName(op.DestinationFile)}");

                        try
                        {
                            EnsureDirectory(Path.GetDirectoryName(op.DestinationFile)!);
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            errorCount++;
                            dialog.AppendLogLine(DistributionLogLevel.Error, $"Ensuring directory failed: {Path.GetDirectoryName(op.DestinationFile)} :: {ex.Message}");
                            continue;
                        }

                        try
                        {
                            if (FilesDifferByHash(op.SourceFile, op.DestinationFile))
                            {
                                File.Copy(op.SourceFile, op.DestinationFile, overwrite: true);
                                copied++;
                                dialog.AppendLogLine(DistributionLogLevel.Info, $"COPY FILE {op.DestinationFile}");
                            }
                            else
                            {
                                skipped++;
                            }
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            errorCount++;
                            dialog.AppendLogLine(DistributionLogLevel.Error, $"Copy failed: {op.SourceFile} -> {op.DestinationFile} :: {ex.Message}");
                        }
                    }

                    // 3) delete obsolete files
                    foreach (var file in plan.FilesToDelete)
                    {
                        Step($"Deleting: {Path.GetFileName(file)}");

                        if (!File.Exists(file))
                        {
                            continue;
                        }

                        try
                        {
                            File.Delete(file);
                            deleted++;
                            dialog.AppendLogLine(DistributionLogLevel.Info, $"DELETE FILE {file}");
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            errorCount++;
                            dialog.AppendLogLine(DistributionLogLevel.Error, $"Delete failed: {file} :: {ex.Message}");
                        }
                    }

                    // 4) remove empty obsolete dirs (bottom-up)
                    foreach (var dir in plan.DirectoriesToDeleteIfEmpty)
                    {
                        Step($"Cleaning directory: {dir}");

                        try
                        {
                            if (!Directory.Exists(dir))
                            {
                                continue;
                            }

                            if (Directory.EnumerateFileSystemEntries(dir).Any())
                            {
                                continue;
                            }

                            Directory.Delete(dir, recursive: false);
                            dialog.AppendLogLine(DistributionLogLevel.Info, $"RMDIR {dir}");
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            errorCount++;
                            dialog.AppendLogLine(DistributionLogLevel.Error, $"Remove directory failed: {dir} :: {ex.Message}");
                        }
                    }

                    var summary = $"Summary: copied={copied}, deleted={deleted}, skipped={skipped}, warnings={warningCount}, errors={errorCount}";

                    dialog.AppendLogLine(DistributionLogLevel.Info, summary);

                    // Final header state
                    if (errorCount > 0)
                    {
                        dialog.MarkCompleted("/Resources/error.png", "Distribute sheets - Error", summary);
                    }
                    else if (warningCount > 0)
                    {
                        dialog.MarkCompleted("/Resources/warning.png", "Distribute sheets - Warning", summary);
                    }
                    else
                    {
                        dialog.MarkCompleted("/Resources/success.png", "Distribute sheets - Success", summary);
                    }

                    // Preserve existing behavior: missing assignments still throw (but after log is visible)
                    if (peopleWithMissingMusicSheets.Any())
                    {
                        throw new MissingMusicSheetException(peopleWithMissingMusicSheets);
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    dialog.AppendLogLine(DistributionLogLevel.Error, "Unhandled exception:");
                    dialog.AppendLogLine(DistributionLogLevel.Error, ex.ToString());

                    dialog.MarkCompleted("/Resources/error.png", "Distribute sheets - Error", "Completed with errors (see log).");
                }
            });

            dialog.ShowDialog();
        }

        public void ExportPartDistribution()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Niklaus Burren");

            using var package = new ExcelPackage();

            // Alle Playlists durchlaufen und jeweils ein eigenes Sheet erstellen
            foreach (var playlist in this.PlaylistService.Playlists)
            {
                // Worksheet mit Playlist-Namen erstellen (ungültige Zeichen entfernen)
                var sheetName = this.SanitizeSheetName(playlist.Name);
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
            }

            // Datei speichern
            var fileInfo = new FileInfo(Path.Combine(Folders.TempFolder, $"Stimmverteilung SGSN {DateTime.Now:G}.xlsx".Replace(":", ".")));
            package.SaveAs(fileInfo);

            Process.Start(new ProcessStartInfo
            {
                FileName = fileInfo.FullName,
                UseShellExecute = true
            });
        }

        #endregion


        private sealed record CopyOperation(string SourceFile, string DestinationFile);

        private sealed record DistributionPlan(
            IReadOnlyList<string> DirectoriesToCreate,
            IReadOnlyList<CopyOperation> FilesToEnsure,
            IReadOnlyList<string> FilesToDelete,
            IReadOnlyList<string> DirectoriesToDeleteIfEmpty);
    }
}

public class MissingMusicSheetException : Exception
{
    #region Constructors

    public MissingMusicSheetException(IEnumerable<Person> people)
        : base(string.Format(Resources.MissingMusicSheetException_Message, string.Join("\n", people.Select(p => $"- {p.FullName} ({p.Instrument})"))))
    {
    }

    #endregion
}
