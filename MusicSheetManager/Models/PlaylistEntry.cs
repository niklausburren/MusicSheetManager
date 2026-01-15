using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MusicSheetManager.Models;

public class PlaylistEntry : ObservableObject
{
    #region Fields

    private int _index;

    #endregion


    #region Constructors

    [JsonConstructor]
    public PlaylistEntry(Guid musicSheetFolderId, bool distribute)
    {
        this.MusicSheetFolderId = musicSheetFolderId;
        this.Distribute = distribute;
    }

    #endregion


    #region Properties

    public Guid MusicSheetFolderId { get; }

    public bool Distribute { get; }

    [JsonIgnore]
    public int Index
    {
        get => _index;
        internal set
        {
            if (this.SetProperty(ref _index, value))
            {
                this.OnPropertyChanged(nameof(this.Number));
            }
        }
    }

    [JsonIgnore]
    public string Number => $"{this.Index + 1:D2}.";

    [JsonIgnore]
    public MusicSheetFolder MusicSheetFolder { get; internal set; }

    #endregion
}