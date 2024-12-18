using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MusicSheetManager.Converters;
using MusicSheetManager.Models;
using MusicSheetManager.Utilities;

namespace MusicSheetManager.Services
{
    public class PeopleService : IPeopleService
    {
        #region Constructors

        public PeopleService()
        {
            this.Options = new JsonSerializerOptions { WriteIndented = true };
            this.Options.Converters.Add(new InstrumentInfoConverter());
            this.Options.Converters.Add(new ClefInfoConverter());
        }

        #endregion


        #region Properties

        private JsonSerializerOptions Options { get; }

        private static string FilePath { get; } = Path.Combine(Folders.AppDataFolder, "people.json");

        #endregion


        #region IPeopleService Members

        public ObservableCollection<Person> People { get; } = new();

        public async Task LoadAsync()
        {
            await using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);

            var people = await JsonSerializer.DeserializeAsync<List<Person>>(stream, this.Options);

            this.People.Clear();

            foreach (var person in people)
            {
                this.People.Add(person);
            }
        }

        public async Task SaveAsync()
        {
            var directory = Path.GetDirectoryName(FilePath);

            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(stream, this.People, this.Options);
        }

        #endregion
    }
}
