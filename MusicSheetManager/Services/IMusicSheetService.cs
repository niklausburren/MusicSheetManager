using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MusicSheetManager.Models;
using MusicSheetManager.ViewModels;

namespace MusicSheetManager.Services;

public interface IMusicSheetService
{
    #region Properties

    ObservableCollection<MusicSheetFolder> MusicSheetFolders { get; }

    #endregion


    #region Public Methods

    Task LoadAsync(IProgress<int> progress);

    Task SplitAsync(string fileName, MusicSheetFolderMetadata metadata, SplitOptions splitOptions, IProgress<(MusicSheet, int)> progress);

    void Import(MusicSheetFolderMetadata metadata, IEnumerable<MusicSheet> musicSheets);

    void SplitPagesFromA3ToA4(string fileName);

    void RotatePages(string fileName);

    #endregion
}
