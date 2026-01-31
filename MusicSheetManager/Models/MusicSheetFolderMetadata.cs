using CommunityToolkit.Mvvm.ComponentModel;

namespace MusicSheetManager.Models;

public class MusicSheetFolderMetadata : ObservableObject
{
    #region Fields

    private string _title;

    private string _composer;

    private string _arranger;

    #endregion


    #region Properties

    public string Title
    {
        get => _title;
        set => this.SetProperty(ref _title, value);
    }

    public string Composer
    {
        get => _composer;
        set => this.SetProperty(ref _composer, value);
    }

    public string Arranger
    {
        get => _arranger;
        set => this.SetProperty(ref _arranger, value);
    }

    #endregion


    #region Public Methods

    public MusicSheetFolderMetadata Clone()
    {
        return new MusicSheetFolderMetadata
        {
            Title = this.Title,
            Composer = this.Composer,
            Arranger = this.Arranger
        };
    }

    #endregion
}