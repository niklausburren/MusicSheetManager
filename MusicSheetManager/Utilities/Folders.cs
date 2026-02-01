using System;
using System.IO;

namespace MusicSheetManager.Utilities
{
    internal static class Folders
    {
        #region Constants

        private const string APP_NAME = "MusicSheetManager";

        #endregion


        #region Properties

        public static string TempFolder { get; } = Path.Combine(Path.GetTempPath(), APP_NAME);

        private static string DefaultAppDataFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), APP_NAME);

        public static string AppDataFolder
        {
            get
            {
                var configured = Properties.Settings.Default.AppDataFolder;
                var path = string.IsNullOrWhiteSpace(configured) ? DefaultAppDataFolder : configured;
                EnsureDirectory(path);
                return path;
            }
        }

        public static string MusicSheetFolder
        {
            get
            {
                var path = Path.Combine(AppDataFolder, "sheets");
                EnsureDirectory(path);
                return path;
            }
        }

        public static string DistributionFolder
        {
            get
            {
                var configured = Properties.Settings.Default.DistributionFolder;
                var path = string.IsNullOrWhiteSpace(configured) ? Path.Combine(AppDataFolder, "distribution") : configured;
                EnsureDirectory(path);
                return path;
            }
        }

        #endregion


        #region Private Methods

        private static void EnsureDirectory(string path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch
            {
                // Swallow exceptions to avoid crashing on invalid paths; caller can handle errors if needed.
            }
        }

        #endregion
    }
}
