using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using MusicSheetManager.Utilities;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace MusicSheetManager.Models
{
    public class MusicSheetFolder : ObservableObject
    {
        #region Constructors

        public MusicSheetFolder(MusicSheetFolderMetadata metadata)
        {
            this.Id = Guid.NewGuid();
            this.Metadata = metadata;

            this.Folder = Path.Combine(Folders.MusicSheetFolder, SanitizeFolderName(this.Metadata.Title));
        }

        public MusicSheetFolder(string folder)
        {
            this.Folder = folder;

            var sheets = Directory.GetFiles(this.Folder, "*.pdf")
                .Select(MusicSheet.Load)
                .ToList();

            foreach (var sheet in sheets)
            {
                this.Sheets.Add(sheet);
                this.SubscribeToSheet(sheet);
            }

            var firstSheet = this.Sheets.First();

            this.Id = firstSheet.FolderId;
            this.Metadata = firstSheet.Metadata.Clone();

            CalculateConflicts(this.Sheets);
        }

        #endregion


        #region Properties

        [PropertyOrder(1)]
        public Guid Id { get; }

        [Browsable(false)]
        public string Folder { get; private set; }

        private MusicSheetFolderMetadata Metadata { get; }

        [Browsable(false)]
        public ObservableCollection<MusicSheet> Sheets { get; } = [];

        [PropertyOrder(2)]
        public string Title
        {
            get => this.Metadata.Title;
            set
            {
                if (this.Metadata.Title == value)
                {
                    return;
                }

                var newFolder = Path.Combine(Folders.MusicSheetFolder, SanitizeFolderName(value));
                
                if (Directory.Exists(newFolder))
                {
                    MessageBox.Show(
                        "A folder with the same name already exists.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                Directory.Move(this.Folder, newFolder);

                this.Metadata.Title = value;

                foreach (var sheet in this.Sheets)
                {
                    sheet.UpdateFolder(newFolder);
                    sheet.Title = value;
                }

                this.Folder = newFolder;
                this.OnPropertyChanged();
            }
        }

        [PropertyOrder(3)]
        public string Composer
        {
            get => this.Metadata.Composer;
            set
            {
                if (this.Metadata.Composer == value)
                {
                    return;
                }

                this.Metadata.Composer = value;

                foreach (var sheet in this.Sheets)
                {
                    sheet.Composer = value;
                }

                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Credits));
            }
        }

        [PropertyOrder(4)]
        public string Arranger
        {
            get => this.Metadata.Arranger;
            set
            {
                if (this.Metadata.Arranger == value)
                {
                    return;
                }

                this.Metadata.Arranger = value;

                foreach (var sheet in this.Sheets)
                {
                    sheet.Arranger = value;
                }

                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Credits));
            }
        }

        [Browsable(false)]
        public string Credits
        {
            get
            {
                var credits = string.Empty;

                if (!string.IsNullOrWhiteSpace(this.Composer))
                {
                    credits += this.Composer;
                }

                if (!string.IsNullOrWhiteSpace(this.Arranger))
                {
                    if (!string.IsNullOrWhiteSpace(credits))
                    {
                        credits += ", ";
                    }

                    credits += $"arr. {this.Arranger}";
                }

                return credits;
            }

        }

        #endregion


        #region Public Methods

        public static MusicSheetFolder Create(MusicSheetFolderMetadata metadata)
        {
            return new MusicSheetFolder(metadata);
        }

        public static MusicSheetFolder TryLoad(string folder)
        {
            return Directory.Exists(folder) && Directory.GetFiles(folder, "*.pdf").Length > 0
                ? new MusicSheetFolder(folder)
                : null;
        }

        public void ImportSheets(IReadOnlyList<MusicSheet> sheets)
        {
            if (sheets == null || sheets.Count == 0)
            {
                throw new ArgumentException("At least one music sheet must be provided.");
            }

            if (sheets.Any(s => s.Instrument == InstrumentInfo.Unknown))
            {
                throw new ArgumentException("All music sheets must have a valid instrument.");
            }

            if (!Directory.Exists(this.Folder))
            {
                Directory.CreateDirectory(this.Folder);
            }

            foreach (var sheet in sheets)
            {
                var duplicate = this.Sheets.FirstOrDefault(s => GetKeyOf(s) == GetKeyOf(sheet));

                if (duplicate != null)
                {
                    var result = MessageBox.Show(
                        Application.Current.MainWindow!,
                        $"The file \"{Path.GetFileName(sheet.FileName)}\" already exists. Do you want to replace it?",
                        "File Exists",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                    {
                        continue;
                    }
                }
                
                sheet.FolderId = this.Id;
                sheet.Title = this.Metadata.Title;
                sheet.Composer = this.Metadata.Composer;
                sheet.Arranger = this.Metadata.Arranger;

                sheet.MoveToFolder(this.Folder);

                if (duplicate != null)
                {
                    this.Sheets.Remove(duplicate);
                }

                this.Sheets.Add(sheet);
                this.SubscribeToSheet(sheet);
            }

            CalculateConflicts(this.Sheets);

            this.OnPropertyChanged(nameof(this.Sheets));
        }

        public static void CalculateConflicts(IReadOnlyList<MusicSheet> musicSheets)
        {
            if (musicSheets == null ||
                musicSheets.Count == 0)
            {
                return;
            }

            var groups = musicSheets
                .GroupBy(GetKeyOf)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var musicSheet in musicSheets)
            {
                var key = GetKeyOf(musicSheet);
                var hasConflict = groups.TryGetValue(key, out var count) && count > 1;
                musicSheet.HasConflict = hasConflict;
            }

            foreach (var musicSheet in musicSheets.Where(s => !s.HasConflict))
            {
                musicSheet.UpdateFileName(onlyIfNumbered: true);
            }
        }

        #endregion


        #region Private Methods

        private static string SanitizeFolderName(string name)
        {
            var sanitized = (name ?? string.Empty).Trim();
            var invalidChars = Path.GetInvalidFileNameChars();
            return invalidChars.Aggregate(sanitized, (current, ch) => current.Replace(ch, '_'));
        }

        private void SubscribeToSheet(MusicSheet sheet)
        {
            sheet.PropertyChanged -= this.OnSheetPropertyChanged;
            sheet.PropertyChanged += this.OnSheetPropertyChanged;

            if (sheet.Parts != null)
            {
                sheet.Parts.CollectionChanged -= this.OnPartsCollectionChanged;
                sheet.Parts.CollectionChanged += this.OnPartsCollectionChanged;
            }
        }

        private static string GetKeyOf(MusicSheet m)
        {
            return $"{m.Instrument?.Key}|{string.Join(",", m.Parts?.OrderBy(p => p.Key).Select(p => p.Key) ?? [])}|{m.Clef?.Key}";
        }

        #endregion


        #region Event Handlers

        private void OnSheetPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is
                nameof(MusicSheet.Instrument) or
                nameof(MusicSheet.Clef) or
                nameof(MusicSheet.Parts))
            {
                CalculateConflicts(this.Sheets);
            }
        }

        private void OnPartsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CalculateConflicts(this.Sheets);
        }

        #endregion
    }
}
