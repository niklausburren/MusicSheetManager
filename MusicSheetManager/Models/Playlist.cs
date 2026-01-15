using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using MusicSheetManager.Services;

namespace MusicSheetManager.Models
{
    public class Playlist : ObservableObject
    {
        #region Constructors

        [JsonConstructor]
        public Playlist(Guid id, string name, ObservableCollection<PlaylistEntry> entries)
        {
            this.Id = id;
            this.Name = name;
            this.Entries = entries;
            this.UpdateIndices();
        }

        #endregion


        #region Properties

        public Guid Id { get; }

        public string Name { get; }

        [JsonIgnore]
        public string SanitizedName
        {
            get
            {
                var invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                var regexSearch = new string(invalidChars);
                var r = new Regex($"[{Regex.Escape(regexSearch)}]");
                return r.Replace(this.Name, "_");
            }
        }

        [JsonIgnore]
        public bool Distribute => this.Entries.Any(e => e.Distribute);

        public ObservableCollection<PlaylistEntry> Entries { get; }

        #endregion


        #region Public Methods

        public void ResolveMusicSheetFolders(IMusicSheetService musicSheetService)
        {
            var folderLookup = musicSheetService.MusicSheetFolders.ToDictionary(f => f.Id);

            foreach (var entry in this.Entries)
            {
                if (folderLookup.TryGetValue(entry.MusicSheetFolderId, out var folder))
                {
                    entry.MusicSheetFolder = folder;
                }
            }
        }

        public void UpdateIndices()
        {
            for (var i = 0; i < this.Entries.Count; i++)
            {
                this.Entries[i].Index = i;
            }
        }

        #endregion
    }
}
