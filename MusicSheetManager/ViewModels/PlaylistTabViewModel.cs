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

    private bool _isFocused;

    #endregion


    #region Constructors

    public PlaylistTabViewModel(IPlaylistService playlistService)
    {
        this.PlaylistService = playlistService;
        this.AddToPlaylistCommand = new RelayCommand<(Playlist playlist, MusicSheetFolder folder)>(this.OnAddToPlaylist);

        this.MovePlaylistEntryUpCommand = new RelayCommand<PlaylistEntry>(this.MoveUp, this.CanMoveUp);
        this.MovePlaylistEntryDownCommand = new RelayCommand<PlaylistEntry>(this.MoveDown, this.CanMoveDown);
    }

    #endregion


    #region Properties

    private IPlaylistService PlaylistService { get; }

    public ObservableCollection<Playlist> Playlists => this.PlaylistService.Playlists;

    public bool IsFocused
    {
        get => _isFocused;
        set => this.SetProperty(ref _isFocused, value);
    }

    public ICommand AddToPlaylistCommand { get; }

    public ICommand MovePlaylistEntryUpCommand { get; }

    public ICommand MovePlaylistEntryDownCommand { get; }

    #endregion


    #region Public Methods

    public async Task InitializeAsync()
    {
        await this.PlaylistService.LoadAsync();
    }

    public void DeletePlaylist(Playlist playlist)
    {
        this.Playlists.Remove(playlist);
        this.PlaylistService.SaveAsync();
    }

    public void DeletePlaylistEntry(PlaylistEntry playlistEntry)
    {
        var playlist = this.Playlists.FirstOrDefault(p => p.Entries.Contains(playlistEntry));

        if (playlist == null)
        {
            return;
        }

        playlist.Entries.Remove(playlistEntry);
        playlist.UpdateIndices();
        this.PlaylistService.SaveAsync();
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
        parameter.playlist.UpdateIndices();
        this.PlaylistService.SaveAsync();
    }

    private void MoveUp(PlaylistEntry entry)
    {
        var playlist = this.Playlists.FirstOrDefault(p => p.Entries.Contains(entry));
        
        if (playlist == null)
        {
            return;
        }

        var index = playlist.Entries.IndexOf(entry);

        if (index > 0)
        {
            playlist.Entries.Move(index, index - 1);
            playlist.UpdateIndices();
            this.PlaylistService.SaveAsync();
        }
    }

    private bool CanMoveUp(PlaylistEntry entry)
    {
        if (entry == null)
        {
            return false;
        }

        var playlist = this.Playlists.FirstOrDefault(p => p.Entries.Contains(entry));

        var index = playlist?.Entries.IndexOf(entry);
        return index > 0;
    }

    private void MoveDown(PlaylistEntry entry)
    {
        var playlist = this.Playlists.FirstOrDefault(p => p.Entries.Contains(entry));

        if (playlist == null)
        {
            return;
        }

        var index = playlist.Entries.IndexOf(entry);

        if (index >= 0 && index < playlist.Entries.Count - 1)
        {
            playlist.Entries.Move(index, index + 1);
            playlist.UpdateIndices();
            this.PlaylistService.SaveAsync();
        }
    }

    private bool CanMoveDown(PlaylistEntry entry)
    {
        var playlist = this.Playlists.FirstOrDefault(p => p.Entries.Contains(entry));

        if (playlist == null)
        {
            return false;
        }

        var index = playlist.Entries.IndexOf(entry);
        return index >= 0 && index < playlist.Entries.Count - 1;
    }

    #endregion
}