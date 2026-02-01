using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicSheetManager.Utilities;

namespace MusicSheetManager.ViewModels
{
    public sealed class OptionsDialogViewModel : ObservableObject
    {
        #region Fields

        private string _appDataFolder;

        private string _distributionFolder;

        private readonly string _originalAppDataFolder;

        #endregion


        #region Events

        public event Action<bool> CloseRequested;

        #endregion


        #region Constructors

        public OptionsDialogViewModel()
        {
            _appDataFolder = NormalizeFolder(Folders.AppDataFolder);
            _distributionFolder = NormalizeFolder(Folders.DistributionFolder);

            _originalAppDataFolder = _appDataFolder;

            this.BrowseAppDataFolderCommand = new RelayCommand(this.BrowseAppDataFolder);
            this.BrowseDistributionFolderCommand = new RelayCommand(this.BrowseDistributionFolder);
            this.SaveCommand = new RelayCommand(this.SaveAndMaybeRestart, this.CanSave);
        }

        #endregion


        #region Properties

        public string AppDataFolder
        {
            get => _appDataFolder;
            set => this.SetProperty(ref _appDataFolder, value);
        }

        public string DistributionFolder
        {
            get => _distributionFolder;
            set => this.SetProperty(ref _distributionFolder, value);
        }

        public ICommand BrowseAppDataFolderCommand { get; }

        public ICommand BrowseDistributionFolderCommand { get; }

        public ICommand SaveCommand { get; }

        #endregion


        #region Private Methods

        private void BrowseAppDataFolder()
        {
            var selected = SelectFolder(this.AppDataFolder);

            if (!string.IsNullOrWhiteSpace(selected))
            {
                this.AppDataFolder = selected;
            }
        }

        private void BrowseDistributionFolder()
        {
            var selected = SelectFolder(this.DistributionFolder);

            if (!string.IsNullOrWhiteSpace(selected))
            {
                this.DistributionFolder = selected;
            }
        }

        private bool CanSave() =>
            !string.IsNullOrWhiteSpace(this.AppDataFolder) &&
            !string.IsNullOrWhiteSpace(this.DistributionFolder);

        private void SaveAndMaybeRestart()
        {
            var normalizedAppDataFolder = NormalizeFolder(this.AppDataFolder);
            var normalizedDistributionFolder = NormalizeFolder(this.DistributionFolder);

            var settings = Properties.Settings.Default;
            var appDataFolderChanged = !string.Equals(normalizedAppDataFolder, _originalAppDataFolder);

            if (appDataFolderChanged)
            {
                var result = MessageBox.Show(
                    "Changing the app data folder requires a restart to reload data.\n\nRestart now?",
                    "Restart required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    settings.AppDataFolder = normalizedAppDataFolder;
                    settings.DistributionFolder = normalizedDistributionFolder;
                    settings.Save();

                    RestartApplication();
                    this.CloseRequested?.Invoke(true);
                    return;
                }

                // Do not change AppDataFolder; still persist DistributionFolder changes
                this.AppDataFolder = _originalAppDataFolder;
                settings.DistributionFolder = normalizedDistributionFolder;
                settings.Save();

                this.CloseRequested?.Invoke(true);
                return;
            }

            // No change that requires restart; persist and close
            settings.AppDataFolder = normalizedAppDataFolder;
            settings.DistributionFolder = normalizedDistributionFolder;
            settings.Save();
            this.CloseRequested?.Invoke(true);
        }

        private static string SelectFolder(string initialPath)
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog
            {
                IsFolderPicker = true,
                EnsurePathExists = true,
                EnsureValidNames = true,
                Title = "Select folder",
                DefaultDirectory = Directory.Exists(initialPath) ? initialPath : ""
            };

            return dialog.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok
                ? dialog.FileName
                : string.Empty;
        }

        private static string NormalizeFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            try
            {
                return Path.GetFullPath(path.Trim());
            }
            catch
            {
                return path.Trim();
            }
        }

        private static void RestartApplication()
        {
            try
            {
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrWhiteSpace(exePath) && File.Exists(exePath))
                {
                    Process.Start(new ProcessStartInfo { FileName = exePath, UseShellExecute = true });
                }
            }
            catch
            {
                // Ignore; still shutting down current app instance
            }
            finally
            {
                Application.Current.Shutdown();
            }
        }

        #endregion
    }
}