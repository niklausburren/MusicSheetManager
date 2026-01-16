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
        private object _activeDocument;

        #endregion


        #region Constructors

        public MainWindowViewModel()
        {
            this.MusicSheetTab = App.Container.Resolve<MusicSheetTabViewModel>();
            this.PeopleTab = App.Container.Resolve<PeopleTabViewModel>();
            this.PlaylistTab = App.Container.Resolve<PlaylistTabViewModel>();
            this.Tools = App.Container.Resolve<ToolsViewModel>();

            this.DeleteSelectedItemCommand = new RelayCommand(this.DeleteSelectedItem, this.CanDeleteSelectedItem);
        }

        #endregion


        #region Properties

        public MusicSheetTabViewModel MusicSheetTab { get; }

        public PeopleTabViewModel PeopleTab { get; }

        public PlaylistTabViewModel PlaylistTab { get; }

        public ToolsViewModel Tools { get; }

        public object SelectedObject
        {
            get => _selectedObject;
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

        private void DeleteSelectedItem()
        {
            switch (this.SelectedObject)
            {
                case Person person when this.PeopleTab.IsFocused:
                    this.PeopleTab.DeletePerson(person);
                    break;
                case Playlist playlist when this.PlaylistTab.IsFocused:
                    this.PlaylistTab.DeletePlaylist(playlist);
                    break;
                case PlaylistEntry playlistEntry when this.PlaylistTab.IsFocused:
                    this.PlaylistTab.DeletePlaylistEntry(playlistEntry);
                    break;
            }

            this.NotifyDeleteCommandChanged();
        }

        private bool CanDeleteSelectedItem()
        {
            return this.SelectedObject switch
            {
                Person _ => this.PeopleTab.IsFocused,
                Playlist _ or PlaylistEntry _ => this.PlaylistTab.IsFocused,
                _ => false
            };
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