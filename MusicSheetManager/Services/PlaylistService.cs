using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MusicSheetManager.Models;
using MusicSheetManager.Utilities;

namespace MusicSheetManager.Services
{
    public class PlaylistService : IPlaylistService
    {
        #region Fields

        private bool _disableAutoSave;

        private Task _saveQueue = Task.CompletedTask;

        private readonly SemaphoreSlim _ioLock = new(1, 1);

        #endregion


        #region Constructors

        public PlaylistService(IMusicSheetService musicSheetService)
        {
            this.MusicSheetService = musicSheetService;
            this.Options = new JsonSerializerOptions { WriteIndented = true };

            this.Playlists.CollectionChanged += this.Playlist_CollectionChanged;
        }

        #endregion


        #region Properties

        private IMusicSheetService MusicSheetService { get; }

        private JsonSerializerOptions Options { get; }

        private static string FilePath { get; } = Path.Combine(Folders.AppDataFolder, "playlists.json");

        #endregion


        #region Private Methods

        private void EnqueueSave()
        {
            lock (_saveQueue)
            {
                _saveQueue = _saveQueue.ContinueWith(
                    async _ =>
                    {
                        try
                        {
                            await this.SaveAsync().ConfigureAwait(false);
                        }
                        catch
                        {
                            // Optional: Logging einbauen
                        }
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.None,
                    TaskScheduler.Default).Unwrap();
            }
        }

        #endregion


        #region Event Handlers

        private void Playlist_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is Playlist p)
                    {
                        p.PropertyChanged += this.Playlist_PropertyChanged;
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is Playlist p)
                    {
                        p.PropertyChanged -= this.Playlist_PropertyChanged;
                    }
                }
            }
        }

        private void Playlist_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_disableAutoSave)
            {
                return;
            }

            this.EnqueueSave();
        }

        #endregion


        #region IPlaylistService Members

        public ObservableCollection<Playlist> Playlists { get; } = [];

        public async Task LoadAsync()
        {
            _disableAutoSave = true;

            await _ioLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (!File.Exists(FilePath))
                {
                    var directory = Path.GetDirectoryName(FilePath);

                    if (directory != null && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    this.Playlists.Clear();
                    return;
                }

                await using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var playlists = await JsonSerializer.DeserializeAsync<List<Playlist>>(stream, this.Options) ?? [];

                this.Playlists.Clear();

                foreach (var playlist in playlists)
                {
                    playlist.ResolveMusicSheetFolders(this.MusicSheetService);
                    this.Playlists.Add(playlist);
                }
            }
            finally
            {
                _disableAutoSave = false;
                _ioLock.Release();
            }
        }

        // SaveAsync bleibt öffentlich, wird aber exklusiv gelockt (Queue ruft es weiterhin auf).
        public async Task SaveAsync()
        {
            var directory = Path.GetDirectoryName(FilePath);

            await _ioLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await using var stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await JsonSerializer.SerializeAsync(stream, this.Playlists, this.Options).ConfigureAwait(false);
            }
            finally
            {
                _ioLock.Release();
            }
        }

        #endregion
    }
}
