using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MusicSheetManager.Models;
using MusicSheetManager.Services;
using MusicSheetManager.Views;

namespace MusicSheetManager.ViewModels;

public class MusicSheetTabViewModel : ObservableObject
{
    #region Constructors

    public MusicSheetTabViewModel(
        IMusicSheetService musicSheetService,
        IMusicSheetAssignmentService musicSheetAssignmentService)
    {
        this.MusicSheetService = musicSheetService;
        this.MusicSheetAssignmentService = musicSheetAssignmentService;

        this.ImportSheetsCommand = new RelayCommand<MusicSheetFolder>(this.ImportSheets); 
        this.AssignMusicSheetsCommand = new RelayCommand<MusicSheetFolder>(this.AssignMusicSheets);
        this.OpenInExplorerCommand = new RelayCommand<MusicSheetFolder>(this.OpenInExplorer, this.CanOpenInExplorer);
        this.CopyFolderIdCommand = new RelayCommand<MusicSheetFolder>(this.CopyFolderId, this.CanCopyFolderId);
    }

    #endregion


    #region Properties

    private IMusicSheetService MusicSheetService { get; }

    private IMusicSheetAssignmentService MusicSheetAssignmentService { get; }

    public ObservableCollection<MusicSheetFolder> MusicSheetFolders => this.MusicSheetService.MusicSheetFolders;

    public ICommand ImportSheetsCommand { get; }

    public ICommand AssignMusicSheetsCommand { get; }

    public ICommand OpenInExplorerCommand { get; }

    public ICommand CopyFolderIdCommand { get; }

    #endregion


    #region Public Methods

    public async Task InitializeAsync()
    {
        await this.MusicSheetService.LoadAsync();
        await this.MusicSheetAssignmentService.LoadAsync();
    }

    #endregion


    #region Private Methods

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
            openFileDialog.FileName,
            musicSheetFolder);
    }

    private void AssignMusicSheets(MusicSheetFolder musicSheetFolder)
    {
        if (musicSheetFolder is null)
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

    private bool CanCopyFolderId(MusicSheetFolder musicSheetFolder)
    {
        return musicSheetFolder != null && musicSheetFolder.Id != Guid.Empty;
    }

    private void CopyFolderId(MusicSheetFolder musicSheetFolder)
    {
        try
        {
            Clipboard.SetText(musicSheetFolder.Id.ToString());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error while copying to clipboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion
}