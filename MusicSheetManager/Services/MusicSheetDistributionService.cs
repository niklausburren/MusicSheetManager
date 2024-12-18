using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MusicSheetManager.Models;
using MusicSheetManager.Properties;
using MusicSheetManager.Utilities;

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
                    var musicSheetFolder = this.MusicSheetService.MusicSheetFolders.SingleOrDefault(f => f.Id == musicSheetFolderId);
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
