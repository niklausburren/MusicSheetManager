using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MusicSheetManager.Models;
using MusicSheetManager.Utilities;

namespace MusicSheetManager.Services
{
    public class MusicSheetAssignmentService : IMusicSheetAssignmentService
    {
        #region Constructors

        public MusicSheetAssignmentService()
        {
            this.Options = new JsonSerializerOptions { WriteIndented = true };
        }

        #endregion


        #region Properties

        private JsonSerializerOptions Options { get; }

        private static string FilePath { get; } = Path.Combine(Folders.AppDataFolder, "assignments.json");

        #endregion


        #region Private Methods

        private static async Task EnsureFileExistsAsync(string path, JsonSerializerOptions options)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var directory = Path.GetDirectoryName(path);

            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(path))
            {
                await using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await JsonSerializer.SerializeAsync(stream, new List<MusicSheetAssignment>(), options).ConfigureAwait(false);
            }
        }

        #endregion


        #region IMusicSheetAssignmentService Members

        /// <inheritdoc />
        public ObservableCollection<MusicSheetAssignment> Assignments { get; } = [];

        public IEnumerable<MusicSheet> GetAssignableMusicSheets(MusicSheetFolder folder, Person person)
        {
            foreach (var instrument in person.Instrument.GetSelfAndFallbacks())
            {
                foreach (var sheet in folder.Sheets.Where(s => s.Instrument == instrument && s.Clef == person.Clef))
                {
                    yield return sheet;
                }
            }
        }

        public MusicSheet GetDefaultMusicSheet(MusicSheetFolder folder, Person person)
        {
            foreach (var instrument in person.Instrument.GetSelfAndFallbacks())
            {
                var assignableMusicSheets = folder.Sheets
                    .Where(s => s.Instrument == instrument && s.Clef == person.Clef)
                    .ToList();

                MusicSheet musicSheet;

                var parts = person.Part != PartInfo.None
                    ? person.Part.GetSelfAndHigherParts()
                    : PartInfo.Fifth.GetSelfAndHigherParts();

                foreach (var part in parts)
                {
                    musicSheet = assignableMusicSheets.FirstOrDefault(s => s.Parts.Contains(part));

                    if (musicSheet != null)
                    {
                        return musicSheet;
                    }
                }

                musicSheet = assignableMusicSheets.FirstOrDefault(s => !s.Parts.Any());

                if (musicSheet != null)
                {
                    return musicSheet;
                }
            }

            return null;
        }

        public MusicSheet GetAssignedMusicSheet(MusicSheetFolder folder, Person person)
        {
            var assignment = this.Assignments.FirstOrDefault(a => a.PersonId == person.Id && a.MusicSheetFolderId == folder.Id);

            return assignment != null
                ? folder.Sheets.FirstOrDefault(s => s.Id == assignment.MusicSheetId)
                : this.GetDefaultMusicSheet(folder, person);
        }

        /// <inheritdoc />
        public async Task LoadAsync()
        {
            await EnsureFileExistsAsync(FilePath, this.Options).ConfigureAwait(false);

            await using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var assignments = await JsonSerializer.DeserializeAsync<List<MusicSheetAssignment>>(stream, this.Options) ?? [];

            this.Assignments.Clear();

            foreach (var assignment in assignments)
            {
                this.Assignments.Add(assignment);
            }
        }

        /// <inheritdoc />
        public async Task SaveAsync()
        {
            var directory = Path.GetDirectoryName(FilePath);

            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(stream, this.Assignments, this.Options).ConfigureAwait(false);
        }

        #endregion
    }
}
