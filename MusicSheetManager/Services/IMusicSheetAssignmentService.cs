using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MusicSheetManager.Models;

namespace MusicSheetManager.Services;

public interface IMusicSheetAssignmentService
{
    #region Properties

    ObservableCollection<MusicSheetAssignment> Assignments { get; }

    #endregion


    #region Public Methods

    IEnumerable<MusicSheet> GetAssignableMusicSheets(MusicSheetFolder folder, Person person);

    MusicSheet GetDefaultMusicSheet(MusicSheetFolder folder, Person person);

    MusicSheet GetAssignedMusicSheet(MusicSheetFolder folder, Person person);

    Task LoadAsync();

    Task SaveAsync();

    #endregion
}