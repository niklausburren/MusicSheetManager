using System.IO;
using System.Threading.Tasks;

namespace MusicSheetManager.Utilities;

public static class FileSystemHelper
{   
    #region Public Methods

    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        var invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        var regexSearch = new string(invalidChars);
        var r = new System.Text.RegularExpressions.Regex($"[{System.Text.RegularExpressions.Regex.Escape(regexSearch)}]");
        return r.Replace(fileName, "_");
    }

    public static async Task<bool> TryDeleteFolderAsync(
        string folder,
        bool changeWorkingDirectory = true,
        int maxRetries = 20,
        int delayMs = 500)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return false;
        }

        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                if (!Directory.Exists(folder))
                {
                    return true;
                }

                var dir = new DirectoryInfo(folder);
                dir.Attributes = FileAttributes.Normal;
                Directory.Delete(folder, recursive: true);
                return true;
            }
            catch
            {
                await Task.Delay(delayMs).ConfigureAwait(false);
            }
        }

        return false;
    }

    public static async Task<bool> TryDeleteFileAsync(
        string fileName,
        bool changeWorkingDirectory = true,
        int maxRetries = 20,
        int delayMs = 500)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                if (!File.Exists(fileName))
                {
                    return true;
                }

                var dir = new FileInfo(fileName);
                dir.Attributes = FileAttributes.Normal;
                File.Delete(fileName);
                return true;
            }
            catch
            {
                await Task.Delay(delayMs).ConfigureAwait(false);
            }
        }

        return false;
    }

    #endregion
}