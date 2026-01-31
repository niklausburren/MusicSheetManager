using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MusicSheetManager.Converters;
using MusicSheetManager.Models;
using MusicSheetManager.Utilities;

namespace MusicSheetManager.Services
{
    public class PeopleService : IPeopleService
    {
        #region Fields

        private bool _disableAutoSave;

        private Task _saveQueue = Task.CompletedTask;

        private readonly SemaphoreSlim _ioLock = new(1, 1);

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


        #region Private Methods

        private void EnqueueSave()
        {
            lock (_saveQueue)
            {
                _saveQueue = _saveQueue.ContinueWith(
                    async _ =>
                    {
                        try
                        {
                            await this.SaveAsync().ConfigureAwait(false);
                        }
                        catch
                        {
                            // Optional: Logging einbauen
                        }
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.None,
                    TaskScheduler.Default).Unwrap();
            }
        }

        #endregion


        #region Event Handlers

        private void People_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
        }

        private void Person_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_disableAutoSave)
            {
                return;
            }

            this.EnqueueSave();
        }

        #endregion


        #region IPeopleService Members

        public ObservableCollection<Person> People { get; } = [];

        public async Task LoadAsync()
        {
            _disableAutoSave = true;

            await _ioLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (!File.Exists(FilePath))
                {
                    var directory = Path.GetDirectoryName(FilePath);

                    if (directory != null && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    this.People.Clear();
                    return;
                }

                await using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var people = await JsonSerializer.DeserializeAsync<List<Person>>(stream, this.Options) ?? [];

                this.People.Clear();

                foreach (var person in people.OrderBy(p => p.FullName))
                {
                    this.People.Add(person);
                }
            }
            finally
            {
                _disableAutoSave = false;
                _ioLock.Release();
            }
        }

        public async Task SaveAsync()
        {
            var directory = Path.GetDirectoryName(FilePath);

            await _ioLock.WaitAsync().ConfigureAwait(false);

            try
            {
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // FileShare.None verhindert parallelen Zugriff anderer Prozesse während des Schreibens
                await using var stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await JsonSerializer.SerializeAsync(stream, this.People, this.Options).ConfigureAwait(false);
            }
            finally
            {
                _ioLock.Release();
            }
        }

        #endregion
    }
}
