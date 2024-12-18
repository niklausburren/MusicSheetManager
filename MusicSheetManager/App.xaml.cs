using Autofac;
using System.Windows;
using MusicSheetManager.Views;

namespace MusicSheetManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IContainer _container;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Autofac konfigurieren
            _container = DependencyConfig.Configure();

            // Hauptfenster aufrufen
            var mainWindow = _container.Resolve<MainWindow>();
            mainWindow.Show();
        }
    }

}
