using System.Windows;
using Autofac;
using MusicSheetManager.Views;

namespace MusicSheetManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Properties

        public static  IContainer Container { get; private set; }

        #endregion


        #region Protected Methods

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Container = DependencyConfig.Configure();

            var mainWindow = Container.Resolve<MainWindow>();
            mainWindow.Show();
        }

        #endregion
    }

}
