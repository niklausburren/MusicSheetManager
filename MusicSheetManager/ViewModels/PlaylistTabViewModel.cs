using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicSheetManager.Models;
using MusicSheetManager.Services;

namespace MusicSheetManager.ViewModels;

public class PlaylistTabViewModel : ObservableObject
{
    #region Events

    public event Action<FocusRequestedEventArgs> FocusRequested;

    #endregion


    #region Constructors

    public PlaylistTabViewModel(IPlaylistService playlistService)
    {
        this.PlaylistService = playlistService;

        this.AddToPlaylistCommand = new AsyncRelayCommand<(Playlist playlist, MusicSheetFolder folder)>(this.AddToPlaylistAsync);
        this.CreatePlaylistCommand = new AsyncRelayCommand(this.CreatePlaylistAsync);
        this.MovePlaylistEntryUpCommand = new AsyncRelayCommand<PlaylistEntry>(this.MoveUpAsync, this.CanMoveUp);
        this.MovePlaylistEntryDownCommand = new AsyncRelayCommand<PlaylistEntry>(this.MoveDownAsync, this.CanMoveDown);
        this.AddPlaceholderCommand = new AsyncRelayCommand<Playlist>(this.AddPlaceholderAsync, this.CanAddPlaceholder);
    }

    #endregion


    #region Properties

    private IPlaylistService PlaylistService { get; }

    public ObservableCollection<Playlist> Playlists => this.PlaylistService.Playlists;

    public ICommand CreatePlaylistCommand { get; }

    public ICommand AddToPlaylistCommand { get; }

    public ICommand MovePlaylistEntryUpCommand { get; }

    public ICommand MovePlaylistEntryDownCommand { get; }

    public ICommand AddPlaceholderCommand { get; }

    #endregion


    #region Public Methods

    public async Task InitializeAsync()
    {
        await this.PlaylistService.LoadAsync();
    }

    public async Task DeletePlaylistAsync(Playlist playlist)
    {
        this.FocusRequested?.Invoke(FocusRequestedEventArgs.Empty);

        var result = MessageBox.Show(
            Application.Current.MainWindow!,
            $"Do you really want to delete playlist \"{playlist.Name}\"?",
            "Delete Person",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.No)
        {
            return;
        }

        this.Playlists.Remove(playlist);
        await this.PlaylistService.SaveAsync();
    }

    public async Task DeletePlaylistEntryAsync(PlaylistEntry playlistEntry)
    {
        this.FocusRequested?.Invoke(FocusRequestedEventArgs.Empty);

        var playlist = this.Playlists.FirstOrDefault(p => p.Entries.Contains(playlistEntry));

        if (playlist == null)
        {
            return;
        }

        playlist.Entries.Remove(playlistEntry);
        playlist.UpdateIndices();
        await this.PlaylistService.SaveAsync();
    }

    #endregion


    #region Private Methods

    private async Task CreatePlaylistAsync()
    {
        var playlist = new Playlist(Guid.NewGuid(), "New Playlist", []);
        this.PlaylistService.Playlists.Add(playlist);
        await this.PlaylistService.SaveAsync();

        this.FocusRequested?.Invoke(new FocusRequestedEventArgs(playlist));
    }

    private async Task AddToPlaylistAsync((Playlist playlist, MusicSheetFolder folder) parameter)
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
        await this.PlaylistService.SaveAsync();
    }

    private async Task AddPlaceholderAsync(Playlist playlist)
    {
        if (playlist == null)
        {
            return;
        }

        var placeholder = new PlaylistEntry(Guid.Empty, distribute: false)
        {
            Index = playlist.Entries.Count,
            MusicSheetFolder = null
        };

        playlist.Entries.Add(placeholder);
        await this.PlaylistService.SaveAsync();
    }

    private bool CanAddPlaceholder(Playlist playlist)
    {
        return playlist != null;
    }

    private async Task MoveUpAsync(PlaylistEntry entry)
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
            await this.PlaylistService.SaveAsync();
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

    private async Task MoveDownAsync(PlaylistEntry entry)
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
            await this.PlaylistService.SaveAsync();
        }
    }

    private bool CanMoveDown(PlaylistEntry entry)
    {
        if (entry == null)
        {
            return false;
        }

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