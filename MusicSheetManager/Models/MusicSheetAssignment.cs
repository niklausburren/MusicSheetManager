using System;
using System.Text.Json.Serialization;

namespace MusicSheetManager.Models
{
    public class MusicSheetAssignment
    {
        #region Constructors

        [JsonConstructor]
        public MusicSheetAssignment(Guid musicSheetFolderId, Guid musicSheetId, Guid personId)
        {
            this.MusicSheetFolderId = musicSheetFolderId;
            this.MusicSheetId = musicSheetId;
            this.PersonId = personId;
        }

        #endregion


        #region Properties

        public Guid MusicSheetFolderId { get; }

        public Guid MusicSheetId { get; }

        public Guid PersonId { get; }

        #endregion
    }
}
