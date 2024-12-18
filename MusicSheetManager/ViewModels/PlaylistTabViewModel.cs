using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MusicSheetManager.Models;
using MusicSheetManager.Services;

namespace MusicSheetManager.ViewModels;

public class PlaylistTabViewModel : ObservableObject
{
    #region Fields

    private Playlist _selectedPlaylist;

    #endregion


    #region Constructors

    public PlaylistTabViewModel(IPlaylistService playlistService)
    {
        this.PlaylistService = playlistService;
    }

    #endregion


    #region Properties

    private IPlaylistService PlaylistService { get; }

    public ObservableCollection<Playlist> Playlists => this.PlaylistService.Playlists;

    public Playlist SelectedPlaylist
    {
        get => _selectedPlaylist;
        set => this.SetProperty(ref _selectedPlaylist, value);
    }

    #endregion


    #region Public Methods

    public async Task InitializeAsync()
    {
        await this.PlaylistService.LoadAsync();
    }

    #endregion
}