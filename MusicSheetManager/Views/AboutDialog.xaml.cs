using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace MusicSheetManager.Views
{
    public partial class AboutDialog : Window
    {
        #region Constructors

        public AboutDialog()
        {
            this.InitializeComponent();
            this.LoadData();
        }

        #endregion


        #region Private Methods

        private void LoadData()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

            AppNameText.Text = GetProductName(assembly) ?? assembly.GetName().Name ?? "Application";

            var version = GetVersion(assembly) ?? assembly.GetName().Version?.ToString();
            VersionText.Text = $"Version: {version ?? "n/a"}";

            var path = assembly.Location;

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                var buildDate = File.GetLastWriteTime(path);
                BuildDateText.Text = $"Build date: {buildDate.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}";
            }
            else
            {
                BuildDateText.Text = "Build date: n/a";
            }

            if (Application.Current?.MainWindow?.Icon != null)
            {
                AppIconImage.Source = Application.Current.MainWindow.Icon;
            }
        }

        private static string GetProductName(Assembly assembly)
        {
            var attr = assembly.GetCustomAttribute<AssemblyProductAttribute>();
            return attr?.Product;
        }

        private static string GetVersion(Assembly assembly)
        {
            var info = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
            return info?.Version;
        }

        #endregion


        #region Event Handlers

        private void GitHubLink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
            }
            catch
            {
                // ignore
            }

            e.Handled = true;
        }

        #endregion
    }
}