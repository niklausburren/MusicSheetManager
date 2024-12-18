using System.Threading.Tasks;
using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MusicSheetManager.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        #region Constructors

        public MainWindowViewModel()
        {
            this.MusicSheetTab = App.Container.Resolve<MusicSheetTabViewModel>();
            this.PeopleTab = App.Container.Resolve<PeopleTabViewModel>();
            this.PlaylistTab = App.Container.Resolve<PlaylistTabViewModel>();
            this.Tools = App.Container.Resolve<ToolsViewModel>();
            
        }

        #endregion


        #region Properties

        public MusicSheetTabViewModel MusicSheetTab { get; }

        public PeopleTabViewModel PeopleTab { get; }

        public PlaylistTabViewModel PlaylistTab { get; }

        public ToolsViewModel Tools { get; }

        #endregion


        #region Public Methods

        public async Task InitializeAsync()
        {
            await this.MusicSheetTab.InitializeAsync();
            await this.PeopleTab.InitializeAsync();
            await this.PlaylistTab.InitializeAsync();
        }

        #endregion
    }
}