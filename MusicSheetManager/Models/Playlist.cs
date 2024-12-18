using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MusicSheetManager.Models
{
    public class Playlist
    {
        #region Constructors

        [JsonConstructor]
        public Playlist(Guid id, string name, IList<Guid> musicSheetFolderIds)
        {
            this.Id = id;
            this.Name = name;
            this.MusicSheetFolderIds = musicSheetFolderIds;
        }

        #endregion

        #region Properties

        public Guid Id { get; }

        public string Name { get; }

        [JsonIgnore]
        public string SanitizedName
        {
            get
            {
                var invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                var regexSearch = new string(invalidChars);
                var r = new Regex($"[{Regex.Escape(regexSearch)}]");
                return r.Replace(this.Name, "_");
            }
        }

        public IList<Guid> MusicSheetFolderIds { get; }

        #endregion
    }
}
