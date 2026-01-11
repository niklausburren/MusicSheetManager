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


        #region IMusicSheetAssignmentService Members

        /// <inheritdoc />
        public ObservableCollection<MusicSheetAssignment> Assignments { get; } = new();

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

                foreach (var part in person.Part?.GetSelfAndHigherParts() ?? PartInfo.Part5.GetSelfAndHigherParts())
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
            await using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);

            var assignments = await JsonSerializer.DeserializeAsync<List<MusicSheetAssignment>>(stream, this.Options);

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

            await using var stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(stream, this.Assignments, this.Options);
        }

        #endregion
    }
}
