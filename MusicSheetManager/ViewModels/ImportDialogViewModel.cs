using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using MusicSheetManager.Models;
using MusicSheetManager.Services;

namespace MusicSheetManager.ViewModels;

public class ImportViewModel : INotifyPropertyChanged
{
    private readonly IMusicSheetImportService _importService;
    private ObservableCollection<MusicSheet> _importedMusicSheets;

    public ImportViewModel(IMusicSheetImportService importService)
    {
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        ImportCommand = new RelayCommand<string>(ExecuteImport, CanExecuteImport);
        ImportedMusicSheets = new ObservableCollection<MusicSheet>();
    }

    public ObservableCollection<MusicSheet> ImportedMusicSheets
    {
        get => _importedMusicSheets;
        private set
        {
            if (_importedMusicSheets != value)
            {
                _importedMusicSheets = value;
                OnPropertyChanged(nameof(ImportedMusicSheets));
            }
        }
    }

    public ICommand ImportCommand { get; }

    private bool CanExecuteImport(string fileName)
    {
        return !string.IsNullOrEmpty(fileName) && File.Exists(fileName) && Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    private void ExecuteImport(string fileName)
    {
        ImportedMusicSheets = _importService.Import(fileName);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

internal class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Predicate<T> _canExecute;

    public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute((T)parameter);
    }

    public void Execute(object parameter)
    {
        _execute((T)parameter);
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}
