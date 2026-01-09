using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MusicSheetManager.Models;
using MusicSheetManager.Services;

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
    }

    #endregion


    #region Properties

    private IMusicSheetService MusicSheetService { get; }

    private IMusicSheetAssignmentService MusicSheetAssignmentService { get; }

    public ObservableCollection<MusicSheetFolder> MusicSheetFolders => this.MusicSheetService.MusicSheetFolders;

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