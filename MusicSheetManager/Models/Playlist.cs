using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using MusicSheetManager.Services;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace MusicSheetManager.Models
{
    public class Playlist : ObservableObject
    {
        #region Fields

        private string _name;

        #endregion


        #region Constructors

        [JsonConstructor]
        public Playlist(Guid id, string name, ObservableCollection<PlaylistEntry> entries)
        {
            this.Id = id;
            this.Name = name;
            this.Entries = entries;
            this.UpdateIndices();

            this.Entries.CollectionChanged += this.OnEntriesCollectionChanged;

            foreach (var entry in this.Entries)
            {
                entry.PropertyChanged += this.OnEntryPropertyChanged;
            }
        }

        #endregion


        #region Properties

        [PropertyOrder(1)]
        [ReadOnly(true)]
        public Guid Id { get; set; }

        [PropertyOrder(2)]
        public string Name
        {
            get => _name;
            set
            {
                if (this.SetProperty(ref _name, value))
                {
                    this.OnPropertyChanged(nameof(this.SanitizedName));
                }
            }
        }

        [PropertyOrder(3)]
        [JsonIgnore]
        public bool Distribute
        {
            get { return this.Entries.Any(e => e.Distribute); }
            set
            {
                foreach (var entry in this.Entries)
                {
                    entry.Distribute = value;
                }

                this.OnPropertyChanged();
            }
        }

        [Browsable(false)]
        public ObservableCollection<PlaylistEntry> Entries { get; set; }

        [Browsable(false)]
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


        #region Event Handlers

        private void OnEntriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (PlaylistEntry oldEntry in e.OldItems)
                {
                    oldEntry.PropertyChanged -= this.OnEntryPropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (PlaylistEntry newEntry in e.NewItems)
                {
                    newEntry.PropertyChanged += this.OnEntryPropertyChanged;
                }
            }

            // Distribute kann sich durch Add/Remove geändert haben
            this.OnPropertyChanged(nameof(this.Distribute));
        }

        private void OnEntryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlaylistEntry.Distribute))
            {
                // Playlist.Distribute neu benachrichtigen
                this.OnPropertyChanged(nameof(this.Distribute));
            }
        }

        #endregion
    }
}
