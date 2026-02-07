using System;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using MusicSheetManager.Services;
using MusicSheetManager.Views;
using SplashScreen = MusicSheetManager.Views.SplashScreen;

namespace MusicSheetManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Properties

        public static IContainer Container { get; private set; }

        #endregion


        #region Protected Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _ = this.RunStartupAsync();
        }

        #endregion


        #region Private Methods

        private async Task RunStartupAsync()
        {
            Container = DependencyConfig.Configure();

            var splash = new SplashScreen();
            splash.Show();

            try
            {
                var musicSheetService = Container.Resolve<IMusicSheetService>();
                var peopleService = Container.Resolve<IPeopleService>();
                var playlistService = Container.Resolve<IPlaylistService>();
                var musicSheetAssignmentService = Container.Resolve<IMusicSheetAssignmentService>();

                var progress = new Progress<int>(p => splash.Progress = p);

                await musicSheetService.LoadAsync(progress);
                await peopleService.LoadAsync();
                await playlistService.LoadAsync();
                await musicSheetAssignmentService.LoadAsync();

                var mainWindow = Container.Resolve<MainWindow>();

                this.MainWindow = mainWindow;
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Music Sheet Manager", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Shutdown(-1);
            }
            finally
            {
                splash.Close();
            }
        }

        #endregion
    }
}
