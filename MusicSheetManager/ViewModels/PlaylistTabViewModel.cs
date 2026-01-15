using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        this.AddToPlaylistCommand = new RelayCommand<(Playlist playlist, MusicSheetFolder folder)>(this.OnAddToPlaylist);
    }

    #endregion


    #region Properties

    private IPlaylistService PlaylistService { get; }

    public ICommand AddToPlaylistCommand { get; }

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


    #region Private Methods

    private void OnAddToPlaylist((Playlist playlist, MusicSheetFolder folder) parameter)
    {
        if (parameter.playlist == null || parameter.folder == null)
        {
            return;
        }

        if (parameter.playlist.Entries.Any(e => e.MusicSheetFolderId == parameter.folder.Id))
        {
            return;
        }

        var newEntry = new PlaylistEntry(parameter.folder.Id, distribute: true)
        {
            Index = parameter.playlist.Entries.Count,
            MusicSheetFolder = parameter.folder
        };

        parameter.playlist.Entries.Add(newEntry);
        this.PlaylistService.SaveAsync();
    }

    #endregion
}