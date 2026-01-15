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

    public bool Distribute { get; }

    [JsonIgnore]
    public int Index { get; internal set; }

    [JsonIgnore]
    public string Number => $"{this.Index + 1:D2}.";

    [JsonIgnore]
    public MusicSheetFolder MusicSheetFolder { get; internal set; }

    #endregion
}