using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using MusicSheetManager.ViewModels;

namespace MusicSheetManager.Views
{
    public partial class DistributionDialog : Window
    {
        #region Constructors

        public DistributionDialog(DistributionDialogViewModel viewModel)
        {
            this.InitializeComponent();

            this.DataContext = viewModel;

            this.Loaded += (_, _) =>
            {
                // Subscribe once UI is ready
                this.ViewModel.LogEntries.CollectionChanged += this.OnLogEntriesCollectionChanged;
            };

            this.Closed += (_, _) =>
            {
                this.ViewModel.LogEntries.CollectionChanged -= this.OnLogEntriesCollectionChanged;
            };

            // Block closing via title bar while running (mirrors Close button enabled state)
            this.Closing += this.DistributionDialog_Closing;
        }

        #endregion


        #region Properties

        public DistributionDialogViewModel ViewModel => (DistributionDialogViewModel)this.DataContext;

        #endregion


        #region Event Handlers

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
                LogListView.ScrollIntoView(last);
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DistributionDialog_Closing(object? sender, CancelEventArgs e)
        {
            // Only allow closing (including title bar X) when not running
            if (this.ViewModel.IsRunning)
            {
                e.Cancel = true;
            }
        }

        #endregion
    }

    public enum DistributionLogLevel
    {
        Info,
        Warning,
        Error
    }

    public sealed class DistributionLogEntry : ObservableObject
    {
        #region Constructors

        public DistributionLogEntry(DistributionLogLevel level, string icon, string text)
        {
            this.Level = level;
            this.Icon = icon;
            this.Text = text;
        }

        #endregion


        #region Properties

        public DistributionLogLevel Level { get; }

        public string Icon { get; }

        public string Text { get; }

        #endregion
    }
}