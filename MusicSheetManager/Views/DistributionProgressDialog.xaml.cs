using System;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MusicSheetManager.Views
{
    public partial class DistributionProgressDialog : Window
    {
        #region Constructors

        public DistributionProgressDialog()
        {
            this.InitializeComponent();
            this.DataContext = new DistributionProgressDialogViewModel();

            this.Loaded += (_, _) =>
            {
                // Subscribe once UI is ready
                this.ViewModel.LogEntries.CollectionChanged += this.OnLogEntriesCollectionChanged;
            };

            this.Closed += (_, _) =>
            {
                this.ViewModel.LogEntries.CollectionChanged -= this.OnLogEntriesCollectionChanged;
            };
        }

        #endregion

        #region Properties

        public DistributionProgressDialogViewModel ViewModel => (DistributionProgressDialogViewModel)this.DataContext;

        #endregion

        #region Public Methods

        public void AppendLogLine(DistributionLogLevel level, string message)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.AppendLogLine(level, message));
                return;
            }

            this.ViewModel.AppendLogEntry(level, message);
        }

        public void ReportProgress(int progress, string statusText)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.ReportProgress(progress, statusText));
                return;
            }

            this.ViewModel.Progress = progress;
            this.ViewModel.StatusText = statusText ?? string.Empty;
        }

        public void SetHeader(string iconUri, string title)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.SetHeader(iconUri, title));
                return;
            }

            this.ViewModel.HeaderIcon = iconUri;
            this.ViewModel.HeaderTitle = title;
        }

        public void MarkCompleted(string iconUri, string title, string finalStatus)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(() => this.MarkCompleted(iconUri, title, finalStatus));
                return;
            }

            this.ViewModel.HeaderIcon = iconUri;
            this.ViewModel.HeaderTitle = title;
            this.ViewModel.StatusText = finalStatus ?? "Completed";
            this.ViewModel.Progress = 100;
            this.ViewModel.CanClose = true;
        }

        #endregion

        #region Private Methods

        private void OnLogEntriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add || this.ViewModel.LogEntries.Count == 0)
            {
                return;
            }

            // Scroll to newest item (bottom)
            var last = this.ViewModel.LogEntries[^1];

            // Use dispatcher to ensure containers are generated
            this.Dispatcher.BeginInvoke(() =>
            {
                this.LogListView.ScrollIntoView(last);
            });
        }

        #endregion

        #region Event Handlers

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion
    }

    public enum DistributionLogLevel
    {
        Info,
        Warning,
        Error
    }

    public sealed class DistributionLogEntryViewModel : ObservableObject
    {
        public DistributionLogEntryViewModel(DistributionLogLevel level, string icon, string text)
        {
            this.Level = level;
            this.Icon = icon;
            this.Text = text;
        }

        public DistributionLogLevel Level { get; }

        public string Icon { get; }

        public string Text { get; }
    }

    public class DistributionProgressDialogViewModel : ObservableObject
    {
        #region Fields

        private int _progress;

        private string _statusText;

        private bool _canClose;

        private string _headerIcon = "/Resources/sync.png";

        private string _headerTitle = "Distribute sheets...";

        #endregion

        #region Properties

        public int Progress
        {
            get => _progress;
            set
            {
                if (this.SetProperty(ref _progress, value))
                {
                    this.OnPropertyChanged(nameof(this.PercentText));
                }
            }
        }

        public string PercentText => $"{this.Progress}%";

        public string StatusText
        {
            get => _statusText;
            set => this.SetProperty(ref _statusText, value);
        }

        public bool CanClose
        {
            get => _canClose;
            set => this.SetProperty(ref _canClose, value);
        }

        public string HeaderIcon
        {
            get => _headerIcon;
            set => this.SetProperty(ref _headerIcon, value);
        }

        public string HeaderTitle
        {
            get => _headerTitle;
            set => this.SetProperty(ref _headerTitle, value);
        }

        public ObservableCollection<DistributionLogEntryViewModel> LogEntries { get; } = [];

        #endregion

        #region Public Methods

        // Backward-compat: infer level from text prefixes
        public void AppendLogLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                this.LogEntries.Add(new DistributionLogEntryViewModel(DistributionLogLevel.Info, "/Resources/info.png", string.Empty));
                return;
            }

            var (level, icon) = GetLevelAndIcon(line);
            this.LogEntries.Add(new DistributionLogEntryViewModel(level, icon, line));
        }

        // Preferred: explicit level
        public void AppendLogEntry(DistributionLogLevel level, string message)
        {
            var icon = level switch
            {
                DistributionLogLevel.Error => "/Resources/error.png",
                DistributionLogLevel.Warning => "/Resources/warning.png",
                _ => "/Resources/info.png"
            };

            this.LogEntries.Add(new DistributionLogEntryViewModel(level, icon, message ?? string.Empty));
        }

        private static (DistributionLogLevel Level, string Icon) GetLevelAndIcon(string line)
        {
            if (line.StartsWith("! ERROR", StringComparison.OrdinalIgnoreCase))
            {
                return (DistributionLogLevel.Error, "/Resources/error.png");
            }

            if (line.StartsWith("! WARN", StringComparison.OrdinalIgnoreCase))
            {
                return (DistributionLogLevel.Warning, "/Resources/warning.png");
            }

            return (DistributionLogLevel.Info, "/Resources/info.png");
        }

        #endregion
    }
}