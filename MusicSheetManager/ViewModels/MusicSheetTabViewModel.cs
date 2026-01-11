using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MusicSheetManager.Models;
using MusicSheetManager.Services;
using MusicSheetManager.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MusicSheetManager.ViewModels;

public class MusicSheetTabViewModel : ObservableObject
{
    #region Constructors

    public MusicSheetTabViewModel(IMusicSheetService musicSheetService, IMusicSheetAssignmentService musicSheetAssignmentService)
    {
        this.MusicSheetService = musicSheetService;
        this.MusicSheetAssignmentService = musicSheetAssignmentService;

        this.ImportSheetsCommand = new RelayCommand<MusicSheetFolder>(this.ImportSheets); 
        this.AssignMusicSheetsCommand = new RelayCommand<MusicSheetFolder>(this.AssignMusicSheets);
    }

    #endregion


    #region Properties

    private IMusicSheetService MusicSheetService { get; }

    private IMusicSheetAssignmentService MusicSheetAssignmentService { get; }

    public ObservableCollection<MusicSheetFolder> MusicSheetFolders => this.MusicSheetService.MusicSheetFolders;

    public ICommand ImportSheetsCommand { get; }

    public ICommand AssignMusicSheetsCommand { get; }

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

    #endregion
}