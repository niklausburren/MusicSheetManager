using MusicSheetManager.Views;

namespace MusicSheetManager.Services
{
    public interface IDistributionReporter
    {
        #region Public Methods

        void SetHeader(string iconUri, string title);

        void ReportProgress(int progress, string statusText);

        void AppendLog(DistributionLogLevel level, string message);

        void MarkCompleted(string iconUri, string title, string finalStatus);

        #endregion
    }
}