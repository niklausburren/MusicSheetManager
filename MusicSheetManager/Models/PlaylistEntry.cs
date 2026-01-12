using System;
using System.Text.Json.Serialization;

namespace MusicSheetManager.Models;

public class PlaylistEntry
{
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

    [JsonIgnore]
    public MusicSheetFolder MusicSheetFolder { get; internal set; }

    public bool Distribute { get; }

    [JsonIgnore]
    public int Index { get; internal set; }

    #endregion
}