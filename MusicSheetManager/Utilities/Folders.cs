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

        public static string AppDataFolder { get; } = "C:\\Users\\BurrNik1\\OneDrive\\Dokumente\\MusicSheetManager"; // Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), APP_NAME);

        public static string MusicSheetFolder { get; } = Path.Combine(AppDataFolder, "sheets");

        public static string DistributionFolder { get; } = "C:\\Users\\nikla\\01_Stimmverteilung\\2026";

        public static string ImportFolder { get; } = Path.Combine(TempFolder, "import");

        #endregion
    }
}
