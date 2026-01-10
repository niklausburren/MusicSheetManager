using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;
using Autofac;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicSheetManager.Models;
using MusicSheetManager.Services;
using MusicSheetManager.Views;

namespace MusicSheetManager.ViewModels;

public class MusicSheetTabViewModel : ObservableObject
{
    #region Fields

    private MusicSheetFolder _selectedMusicSheetFolder;

    #endregion


    #region Constructors

    public MusicSheetTabViewModel(IMusicSheetService musicSheetService, IMusicSheetAssignmentService musicSheetAssignmentService)
    {
        this.MusicSheetService = musicSheetService;
        this.MusicSheetAssignmentService = musicSheetAssignmentService;

        this.ImportSheetsCommand = new RelayCommand<MusicSheetFolder>(this.ImportSheets); 
        this.AssignMusicSheetsCommand = new RelayCommand<MusicSheetFolder>(this.AssignMusicSheets);
    }

    private void ImportSheets(MusicSheetFolder musicSheetFolder)
    {
        if (musicSheetFolder is null)
        {
            return;
        }

        var importDialog = App.Container.Resolve<ImportDialog>();
        importDialog.Owner = System.Windows.Application.Current.MainWindow;
        importDialog.ShowDialog();
    }

    private void AssignMusicSheets(MusicSheetFolder musicSheetFolder)
    {
        
    }

    #endregion


    #region Properties

    private IMusicSheetService MusicSheetService { get; }

    private IMusicSheetAssignmentService MusicSheetAssignmentService { get; }

    public ObservableCollection<MusicSheetFolder> MusicSheetFolders => this.MusicSheetService.MusicSheetFolders;

    public ICommand ImportSheetsCommand { get; }

    public ICommand AssignMusicSheetsCommand { get; }

    public MusicSheetFolder SelectedMusicSheetFolder
    {
        get => _selectedMusicSheetFolder;
        set => this.SetProperty(ref _selectedMusicSheetFolder, value);
    }

    #endregion


    #region Public Methods

    public async Task InitializeAsync()
    {
        await this.MusicSheetService.LoadAsync();
        await this.MusicSheetAssignmentService.LoadAsync();
    }

    #endregion
}