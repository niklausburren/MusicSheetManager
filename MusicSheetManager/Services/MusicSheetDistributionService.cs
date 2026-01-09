using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MusicSheetManager.Models;
using MusicSheetManager.Properties;
using MusicSheetManager.Utilities;
using OfficeOpenXml;

namespace MusicSheetManager.Services
{
    public class MusicSheetDistributionService : IMusicSheetDistributionService
    {
        #region Constructors

        public MusicSheetDistributionService(IMusicSheetService musicSheetService, IMusicSheetAssignmentService musicSheetAssignmentService, IPeopleService peopleService, IPlaylistService playlistService)
        {
            this.MusicSheetService = musicSheetService;
            this.MusicSheetAssignmentService = musicSheetAssignmentService;
            this.PeopleService = peopleService;
            this.PlaylistService = playlistService;
        }

        #endregion


        #region Properties

        private IMusicSheetService MusicSheetService { get; }

        private IMusicSheetAssignmentService MusicSheetAssignmentService { get; }

        private IPeopleService PeopleService { get; }

        private IPlaylistService PlaylistService { get; }

        #endregion


        #region Private Methods

        private string SanitizeSheetName(string name)
        {
            // Excel Sheet-Namen dürfen max. 31 Zeichen haben und keine ungültigen Zeichen enthalten
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

        #endregion


        #region IMusicSheetDistributionService Members

        public void Distribute()
        {
            if (Directory.Exists(Folders.DistributionFolder))
            {
                Directory.Delete(Folders.DistributionFolder, true);
            }

            Directory.CreateDirectory(Folders.DistributionFolder);

            var instruments = this.PeopleService.People.Select(p => p.Instrument).Distinct().ToList();
            var peopleWithMissingMusicSheets = new HashSet<Person>();
            
            foreach (var instrument in instruments.Where(i => i.Category != InstrumentCategory.Percussion))
            {
                foreach (var person in this.PeopleService.People.Where(p => p.Instrument == instrument))
                {
                    if (person.Dispensed)
                    {
                        continue;
                    }

                    var personFolder = Path.Combine(Folders.DistributionFolder, $"{instrument.Index:D2} {instrument.DisplayName}", person.FullName);
                    Directory.CreateDirectory(personFolder);

                    foreach (var playlist in this.PlaylistService.Playlists)
                    {
                        var playlistFolder = Path.Combine(personFolder, playlist.SanitizedName);
                        Directory.CreateDirectory(playlistFolder);

                        foreach (var musicSheetFolderId in playlist.MusicSheetFolderIds)
                        {
                            if (musicSheetFolderId == Guid.Empty)
                            {
                                continue;
                            }

                            var number = playlist.MusicSheetFolderIds.IndexOf(musicSheetFolderId) + 1;
                            var musicSheetFolder = this.MusicSheetService.MusicSheetFolders.SingleOrDefault(f => f.Id == musicSheetFolderId);
                            var musicSheet = this.MusicSheetAssignmentService.GetAssignedMusicSheet(musicSheetFolder, person);

                            if (musicSheet != null)
                            {
                                File.Copy(musicSheet.FileName, Path.Combine(playlistFolder, $"{number:D2} {Path.GetFileName(musicSheet.FileName)}"));
                            }
                            else
                            {
                                peopleWithMissingMusicSheets.Add(person);
                            }
                        }
                    }
                }
            }

            foreach (var playlist in this.PlaylistService.Playlists)
            {
                var percussionFolder = Path.Combine(Folders.DistributionFolder, $"{InstrumentInfo.Percussion.Index:D2} {InstrumentInfo.Percussion.DisplayName}", playlist.SanitizedName);
                Directory.CreateDirectory(percussionFolder);

                foreach (var musicSheetFolderId in playlist.MusicSheetFolderIds)
                {
                    if (musicSheetFolderId == Guid.Empty)
                    {
                        continue;
                    }

                    var number = playlist.MusicSheetFolderIds.IndexOf(musicSheetFolderId) + 1;
                    var musicSheetFolder = this.MusicSheetService.MusicSheetFolders.Single(f => f.Id == musicSheetFolderId);
                    var percussionSubFolder = Path.Combine(percussionFolder, $"{number:D2} {musicSheetFolder.Title}");
                    Directory.CreateDirectory(percussionSubFolder);

                    foreach (var musicSheet in musicSheetFolder.Sheets.Where(s => s.Instrument.Category == InstrumentCategory.Percussion))
                    {
                        File.Copy(musicSheet.FileName, Path.Combine(percussionSubFolder, $"{number:D2} {Path.GetFileName(musicSheet.FileName)}"));
                    }
                }
            }

            if (peopleWithMissingMusicSheets.Any())
            {
                throw new MissingMusicSheetException(peopleWithMissingMusicSheets);
            }
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

                foreach (var musicSheetFolderId in playlist.MusicSheetFolderIds)
                {
                    if (musicSheetFolderId == Guid.Empty)
                        continue;

                    var musicSheetFolder = this.MusicSheetService.MusicSheetFolders
                        .SingleOrDefault(f => f.Id == musicSheetFolderId);
                    if (musicSheetFolder == null)
                        continue;

                    musicSheetFolders.Add(musicSheetFolder);
                    worksheet.Cells[headerRow, column].Value = musicSheetFolder.Title;
                    column++;
                }

                // Header formatieren
                using (var range = worksheet.Cells[headerRow, 1, headerRow, column - 1])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

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
