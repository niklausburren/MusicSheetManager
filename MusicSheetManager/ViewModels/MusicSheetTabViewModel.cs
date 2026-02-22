using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MusicSheetManager.Models;
using MusicSheetManager.Services;
using MusicSheetManager.Utilities;
using MusicSheetManager.Views;

namespace MusicSheetManager.ViewModels;

public class MusicSheetTabViewModel : ObservableObject
{
    #region Events

    public event Action FocusRequested;

    #endregion


    #region Constructors

    public MusicSheetTabViewModel(
        IMusicSheetService musicSheetService)
    {
        this.MusicSheetService = musicSheetService;

        this.ImportMusicSheetFolderFromSingleFileCommand = new RelayCommand(this.ImportMusicSheetFolderFromSingleFile);
        this.ImportMusicSheetFolderFromMultipleFilesCommand = new RelayCommand(this.ImportMusicSheetFolderFromMultipleFiles);
        this.ImportSheetsCommand = new RelayCommand<MusicSheetFolder>(this.ImportSheets); 
        this.AssignMusicSheetsCommand = new RelayCommand<object>(this.AssignMusicSheets, this.CanAssignMusicSheets);
        this.OpenInExplorerCommand = new RelayCommand<MusicSheetFolder>(this.OpenInExplorer, this.CanOpenInExplorer);
    }

    #endregion


    #region Properties

    private IMusicSheetService MusicSheetService { get; }

    public ObservableCollection<MusicSheetFolder> MusicSheetFolders => this.MusicSheetService.MusicSheetFolders;

    public ICommand ImportSheetsCommand { get; }

    public ICommand AssignMusicSheetsCommand { get; }

    public ICommand OpenInExplorerCommand { get; }

    public ICommand ImportMusicSheetFolderFromSingleFileCommand { get; }

    public ICommand ImportMusicSheetFolderFromMultipleFilesCommand { get; }

    #endregion


    #region Public Methods

    public async Task DeleteMusicSheetFolderAsync(MusicSheetFolder musicSheetFolder)
    {
        if (musicSheetFolder is null)
        {
            return;
        }

        var dialogResult = MessageBox.Show(
            Application.Current.MainWindow!,
            $"Do you want to delete the music sheet folder \"{musicSheetFolder.Title}\" and all its contents?",
            "Confirm Deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (dialogResult != MessageBoxResult.Yes)
        {
            return;
        }

        var deleteResult = await FileSystemHelper.TryDeleteFolderAsync(musicSheetFolder.Folder);

        if (deleteResult.Success)
        {
            this.MusicSheetService.MusicSheetFolders.Remove(musicSheetFolder);
        }
        else
        {
            MessageBox.Show(
                Application.Current.MainWindow!,
                $"Failed to delete the music sheet folder: {deleteResult.ErrorMessage}",
                "Deletion Failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public async Task DeleteMusicSheetAsync(MusicSheet musicSheet)
    {
        if (musicSheet is null)
        {
            return;
        }

        var dialogResult = MessageBox.Show(
            Application.Current.MainWindow!,
            $"Do you want to delete the music sheet \"{musicSheet.DisplayName}\"?",
            "Confirm Deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (dialogResult != MessageBoxResult.Yes)
        {
            return;
        }

        var folder = this.MusicSheetService.MusicSheetFolders.First(m => m.Id == musicSheet.FolderId);

        var deleteResult = await FileSystemHelper.TryDeleteFileAsync(musicSheet.FileName);

        if (deleteResult.Success)
        {
            folder.Sheets.Remove(musicSheet);
        }
        else
        {
            MessageBox.Show(
                Application.Current.MainWindow!,
                $"Failed to delete the music sheet: {deleteResult.ErrorMessage}",
                "Deletion Failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    #endregion


    #region Private Methods

    private void ImportMusicSheetFolderFromSingleFile()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            Title = "Select a PDF file"
        };

        if (openFileDialog.ShowDialog(Application.Current.MainWindow!) != true)
        {
            return;
        }

        var importDialog = App.Container.Resolve<ImportDialog>();

        importDialog.ShowDialog(
            Application.Current.MainWindow!,
            ImportMode.SplitAndDetect,
            [openFileDialog.FileName]);

        this.FocusRequested?.Invoke();
    }

    private void ImportMusicSheetFolderFromMultipleFiles()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            Title = "Select multiple PDF files",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog(Application.Current.MainWindow!) != true)
        {
            return;
        }

        var importDialog = App.Container.Resolve<ImportDialog>();

        importDialog.ShowDialog(
            Application.Current.MainWindow!,
            ImportMode.DetectOnly,
            openFileDialog.FileNames);

        this.FocusRequested?.Invoke();
    }

    private void ImportSheets(MusicSheetFolder musicSheetFolder)
    {
        if (musicSheetFolder is null)
        {
            return;
        }

        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            Title = "Select a PDF file"
        };

        if (openFileDialog.ShowDialog(Application.Current.MainWindow) != true)
        {
            return;
        }

        var importDialog = App.Container.Resolve<ImportDialog>();
        importDialog.ShowDialog(
            Application.Current.MainWindow,
            ImportMode.SplitAndDetect,
            [openFileDialog.FileName],
            musicSheetFolder);

        this.FocusRequested?.Invoke();
    }

    private bool CanAssignMusicSheets(object parameter)
    {
        return parameter is MusicSheetFolder musicSheetFolder && musicSheetFolder.Sheets.Any();
    }

    private void AssignMusicSheets(object parameter)
    {
        if (parameter is not MusicSheetFolder musicSheetFolder)
        {
            return;
        }

        var assignmentsDialog = App.Container.Resolve<AssignmentsDialog>();
        assignmentsDialog.ShowDialog(
            Application.Current.MainWindow,
            musicSheetFolder);
    }

    private bool CanOpenInExplorer(MusicSheetFolder musicSheetFolder)
    {
        return musicSheetFolder != null && !string.IsNullOrEmpty(musicSheetFolder.Folder) && Directory.Exists(musicSheetFolder.Folder);
    }

    private void OpenInExplorer(MusicSheetFolder musicSheetFolder)
    {
        try
        {
            if (Directory.Exists(musicSheetFolder.Folder))
            {
                Process.Start("explorer.exe", musicSheetFolder.Folder);
            }
            else
            {
                MessageBox.Show($"The directory '{musicSheetFolder.Folder}' does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error while opening explorer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion
}