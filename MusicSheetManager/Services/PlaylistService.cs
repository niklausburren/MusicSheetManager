using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MusicSheetManager.Converters;
using MusicSheetManager.Models;
using MusicSheetManager.Utilities;

namespace MusicSheetManager.Services
{
    public class PlaylistService : IPlaylistService
    {
        #region Constructors

        public PlaylistService()
        {
            this.Options = new JsonSerializerOptions { WriteIndented = true };
        }

        #endregion


        #region Properties

        private JsonSerializerOptions Options { get; }

        private static string FilePath { get; } = Path.Combine(Folders.AppDataFolder, "playlists.json");

        #endregion


        #region IPeopleService Members

        public ObservableCollection<Playlist> Playlists { get; } = new();

        public async Task LoadAsync()
        {
            await using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);

            var playlists = await JsonSerializer.DeserializeAsync<List<Playlist>>(stream, this.Options);

            this.Playlists.Clear();

            foreach (var playlist in playlists)
            {
                this.Playlists.Add(playlist);
            }
        }

        public async Task SaveAsync()
        {
            var directory = Path.GetDirectoryName(FilePath);

            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(stream, this.Playlists, this.Options);
        }

        #endregion
    }
}
