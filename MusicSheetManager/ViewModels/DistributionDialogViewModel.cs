using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicSheetManager.Services;
using MusicSheetManager.Views;

namespace MusicSheetManager.ViewModels;

public class DistributionDialogViewModel : ObservableObject, IDistributionReporter
{
    #region Fields

    private readonly IMusicSheetDistributionService _service;

    private int _progress;

    private string _statusText = "Press start to distribute";

    private string _headerIcon = "/Resources/play.png";

    private string _headerTitle = "Sheet distribution";

    private bool _isRunning;

    private CancellationTokenSource? _cts;

    #endregion


    #region Constructors

    public DistributionDialogViewModel(IMusicSheetDistributionService service)
    {
        _service = service;
        this.StartCommand = new AsyncRelayCommand(this.StartAsync, () => !this.IsRunning);
        this.CancelCommand = new RelayCommand(this.Cancel, () => this.IsRunning);
    }

    #endregion


    #region Properties

    public int Progress
    {
        get { return _progress; }
        set
        {
            if (this.SetProperty(ref _progress, value))
            {
                this.OnPropertyChanged(nameof(this.PercentText));
            }
        }
    }

    public string PercentText
    {
        get { return $"{this.Progress}%"; }
    }

    public string StatusText
    {
        get { return _statusText; }
        set { this.SetProperty(ref _statusText, value); }
    }

    // Close is enabled whenever not running
    public bool CanClose
    {
        get { return !this.IsRunning; }
    }

    public string HeaderIcon
    {
        get { return _headerIcon; }
        set { this.SetProperty(ref _headerIcon, value); }
    }

    public string HeaderTitle
    {
        get { return _headerTitle; }
        set { this.SetProperty(ref _headerTitle, value); }
    }

    public ObservableCollection<DistributionLogEntry> LogEntries { get; } = [];

    public IAsyncRelayCommand StartCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public bool IsRunning
    {
        get { return _isRunning; }
        private set
        {
            if (this.SetProperty(ref _isRunning, value))
            {
                this.StartCommand.NotifyCanExecuteChanged();
                this.CancelCommand.NotifyCanExecuteChanged();
                this.OnPropertyChanged(nameof(this.CanClose));
            }
        }
    }

    #endregion


    #region Private Methods

    private async System.Threading.Tasks.Task StartAsync()
    {
        // Reset UI state for a new run
        this.IsRunning = true;
        this.Progress = 0;
        this.StatusText = string.Empty;
        this.LogEntries.Clear();

        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        try
        {
            await _service.DistributeAsync(this, _cts.Token);
        }
        catch
        {
            // Exceptions are already logged and final status marked by the service.
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;

            // Ensure not running anymore
            this.IsRunning = false;
        }
    }

    private void Cancel()
    {
        _cts?.Cancel();
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

    private static void Dispatch(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;

        if (dispatcher == null || dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            dispatcher.Invoke(action, DispatcherPriority.Background);
        }
    }

    #endregion


    #region IDistributionReporter Members

    public void SetHeader(string iconUri, string title)
    {
        Dispatch(() =>
        {
            this.HeaderIcon = iconUri;
            this.HeaderTitle = title;
        });
    }

    public void ReportProgress(int progress, string statusText)
    {
        Dispatch(() =>
        {
            this.Progress = progress;
            this.StatusText = statusText ?? string.Empty;
        });
    }

    public void AppendLog(DistributionLogLevel level, string message)
    {
        Dispatch(() =>
        {
            var icon = level switch
            {
                DistributionLogLevel.Error => "/Resources/error.png",
                DistributionLogLevel.Warning => "/Resources/warning.png",
                _ => "/Resources/info.png"
            };

            this.LogEntries.Add(new DistributionLogEntry(level, icon, message ?? string.Empty));
        });
    }

    public void MarkCompleted(string iconUri, string title, string finalStatus)
    {
        Dispatch(() =>
        {
            this.HeaderIcon = iconUri;
            this.HeaderTitle = title;
            this.StatusText = finalStatus ?? "Completed";
            this.Progress = 100;
            this.IsRunning = false;
        });
    }

    #endregion
}