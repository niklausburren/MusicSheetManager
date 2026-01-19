using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace MusicSheetManager.Models;

public class PlaylistEntry : ObservableObject
{
    #region Fields

    private int _index;

    private bool _distribute;

    private MusicSheetFolder _musicSheetFolder;

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

    [DisplayName("Id")]
    [PropertyOrder(1)]
    [ReadOnly(true)]
    public Guid MusicSheetFolderId { get; }

    [PropertyOrder(2)]
    [ReadOnly(true)]
    public string Title => this.MusicSheetFolder?.Title ?? "Placeholder";

    [JsonIgnore]
    [PropertyOrder(3)]
    [ReadOnly(true)]
    public string Number => $"{this.Index + 1:D2}";

    [PropertyOrder(4)]
    public bool Distribute
    {
        get => _distribute && this.MusicSheetFolder != null;
        set => this.SetProperty(ref _distribute, value);
    }

    [Browsable(false)]
    [JsonIgnore]
    public MusicSheetFolder MusicSheetFolder
    {
        get => _musicSheetFolder;
        set
        {
            if (this.SetProperty(ref _musicSheetFolder, value))
            {
                this.OnPropertyChanged(nameof(this.Title));
            }
        }
    }

    [Browsable(false)]
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

    #endregion
}