using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicSheetManager.Models;

namespace MusicSheetManager.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        #region Fields

        private object _selectedObject;

        #endregion


        #region Constructors

        public MainWindowViewModel()
        {
            this.MusicSheetTab = App.Container.Resolve<MusicSheetTabViewModel>();
            this.PeopleTab = App.Container.Resolve<PeopleTabViewModel>();
            this.PlaylistTab = App.Container.Resolve<PlaylistTabViewModel>();
            this.Tools = App.Container.Resolve<ToolsViewModel>();

            this.DeleteSelectedItemCommand = new AsyncRelayCommand(this.DeleteSelectedItem, this.CanDeleteSelectedItem);
        }

        #endregion


        #region Properties

        public MusicSheetTabViewModel MusicSheetTab { get; }

        public PeopleTabViewModel PeopleTab { get; }

        public PlaylistTabViewModel PlaylistTab { get; }

        public ToolsViewModel Tools { get; }

        public object SelectedObject
        {
            get { return _selectedObject; }
            set
            {
                if (this.SetProperty(ref _selectedObject, value))
                {
                    this.NotifyDeleteCommandChanged();
                }
            }
        }

        public ICommand DeleteSelectedItemCommand { get; }

        #endregion


        #region Public Methods

        public async Task InitializeAsync()
        {
            await this.MusicSheetTab.InitializeAsync();
            await this.PeopleTab.InitializeAsync();
            await this.PlaylistTab.InitializeAsync();
        }

        #endregion


        #region Private Methods

        private async Task DeleteSelectedItem()
        {
            switch (this.SelectedObject)
            {
                case MusicSheetFolder musicSheetFolder:
                    await this.MusicSheetTab.DeleteMusicSheetFolderAsync(musicSheetFolder);
                    break;
                case MusicSheet musicSheet:
                    await this.MusicSheetTab.DeleteMusicSheetAsync(musicSheet);
                    break;
                case Person person:
                    await this.PeopleTab.DeletePersonAsync(person);
                    break;
                case Playlist playlist:
                    await this.PlaylistTab.DeletePlaylistAsync(playlist);
                    break;
                case PlaylistEntry playlistEntry:
                    await this.PlaylistTab.DeletePlaylistEntryAsync(playlistEntry);
                    break;
            }

            this.NotifyDeleteCommandChanged();
        }

        private bool CanDeleteSelectedItem()
        {
            return this.SelectedObject is MusicSheetFolder or MusicSheet or Playlist or PlaylistEntry or Person;
        }

        private void NotifyDeleteCommandChanged()
        {
            if (this.DeleteSelectedItemCommand is RelayCommand rc)
            {
                rc.NotifyCanExecuteChanged();
            }
        }

        #endregion
    }
}