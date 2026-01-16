using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MusicSheetManager.Converters;
using MusicSheetManager.Models;
using MusicSheetManager.Utilities;

namespace MusicSheetManager.Services
{
    public class PeopleService : IPeopleService
    {
        #region Fields

        // Skip auto-save while loading
        private bool _disableAutoSave;

        #endregion


        #region Constructors

        public PeopleService()
        {
            this.Options = new JsonSerializerOptions { WriteIndented = true };
            this.Options.Converters.Add(new InstrumentInfoConverter());
            this.Options.Converters.Add(new ClefInfoConverter());

            this.People.CollectionChanged += this.People_CollectionChanged;
        }

        #endregion


        #region Properties

        private JsonSerializerOptions Options { get; }

        private static string FilePath { get; } = Path.Combine(Folders.AppDataFolder, "people.json");

        #endregion


        #region Event Handlers

        private void People_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is Person p)
                    {
                        p.PropertyChanged += this.Person_PropertyChanged;
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is Person p)
                    {
                        p.PropertyChanged -= this.Person_PropertyChanged;
                    }
                }
            }

            if (_disableAutoSave)
            {
                return;
            }

            _ = this.SaveAsync();
        }

        private void Person_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_disableAutoSave)
            {
                return;
            }

            _ = this.SaveAsync();
        }

        #endregion


        #region IPeopleService Members

        public ObservableCollection<Person> People { get; } = [];

        public async Task LoadAsync()
        {
            _disableAutoSave = true;

            try
            {
                await using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
                var people = await JsonSerializer.DeserializeAsync<List<Person>>(stream, this.Options);

                this.People.Clear();

                foreach (var person in people.OrderBy(p => p.FullName))
                {
                    this.People.Add(person); // hooks added via CollectionChanged
                }
            }
            finally
            {
                _disableAutoSave = false;
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
