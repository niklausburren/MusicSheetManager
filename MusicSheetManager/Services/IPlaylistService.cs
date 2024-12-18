using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MusicSheetManager.Models;

namespace MusicSheetManager.Services
{
    public interface IPlaylistService
    {
        #region Properties

        ObservableCollection<Playlist> Playlists { get; }

        #endregion


        #region Public Methods

        Task LoadAsync();

        Task SaveAsync();

        #endregion
    }
}
