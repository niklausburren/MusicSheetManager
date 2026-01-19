using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MusicSheetManager.Models;
using MusicSheetManager.Services;

namespace MusicSheetManager.ViewModels
{
    public class ImportDialogViewModel : ObservableObject
    {
        #region Fields

        private readonly IMusicSheetService _musicSheetService;

        private int _progress;

        private string _fileName;

        private MusicSheetFolderMetadata _metadata;

        private SplitOptions _splitOptions;

        private bool _isSplitting;

        #endregion


        #region Constructors

        public ImportDialogViewModel(IMusicSheetService importService)
        {
            _musicSheetService = importService ?? throw new ArgumentNullException(nameof(importService));

            this.Metadata = new MusicSheetFolderMetadata();

            this.SplitOptions = new SplitOptions();
            this.SplitCommand = new AsyncRelayCommand(this.ExecuteSplitAsync, this.CanExecuteSplit);
            this.ImportCommand = new RelayCommand(this.ExecuteImport, this.CanExecuteImport);

            this.MusicSheets.CollectionChanged += this.OnMusicSheetsCollectionChanged;
        }

        #endregion


        #region Properties

        public int Progress
        {
            get => _progress;
            set => this.SetProperty(ref _progress, value);
        }

        public string FileName
        {
            get => _fileName;
            set
            {
                if (this.SetProperty(ref _fileName, value))
                {
                    ((AsyncRelayCommand)this.SplitCommand).NotifyCanExecuteChanged();
                }
            }
        }

        public MusicSheetFolderMetadata Metadata
        {
            get => _metadata;
            set => this.SetProperty(ref _metadata, value);
        }

        public SplitOptions SplitOptions
        {
            get => _splitOptions;
            set => this.SetProperty(ref _splitOptions, value);
        }

        public ICommand SplitCommand { get; }

        public bool IsSplitting
        {
            get => _isSplitting;
            set => this.SetProperty(ref _isSplitting, value);
        }

        public ObservableCollection<MusicSheetInfo> MusicSheets { get; } = [];

        public ICommand ImportCommand { get; }

        public Action<bool?> SetDialogResultAction { get; set; }

        public bool IsMetadataReadOnly => this.MusicSheets?.Count > 0;

        #endregion


        #region Private Methods

        private bool CanExecuteSplit()
        {
            return
                !string.IsNullOrEmpty(this.FileName) &&
                File.Exists(this.FileName) &&
                Path.GetExtension(this.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        private async Task ExecuteSplitAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Metadata.Title))
            {
                MessageBox.Show("Please enter a title under Metadata.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.IsSplitting = true;

            try
            {
                this.Progress = 0;
                this.MusicSheets.Clear();

                var progress = new Progress<(MusicSheet, int)>(report =>
                {
                    var (musicSheet, totalSheets) = report;
                    var info = new MusicSheetInfo(musicSheet);
                    this.SubscribeToItem(info);
                    this.MusicSheets.Add(info);
                    this.Progress = this.MusicSheets.Count * 100 / totalSheets;

                    this.CalculateConflicts();

                    ((RelayCommand)this.ImportCommand).NotifyCanExecuteChanged();
                    this.OnPropertyChanged(nameof(this.IsMetadataReadOnly));
                });

                await _musicSheetService.SplitAsync(this.FileName, this.Metadata, this.SplitOptions, progress).ConfigureAwait(false);
            }
            finally
            {
                this.IsSplitting = false;
            }
        }

        private bool CanExecuteImport()
        {
            var selected = this.MusicSheets.Where(msi => msi.IsSelected).ToList();

            if (selected.Count == 0)
            {
                return false;
            }

            return selected.All(msi =>
                msi.Instrument != InstrumentInfo.Unknown &&
                !msi.Sheet.HasConflict);
        }

        private void ExecuteImport()
        {
            try
            {
                var selectedSheets = this.MusicSheets
                    .Where(msi => msi.IsSelected)
                    .Select(msi => msi.Sheet)
                    .ToList();

                _musicSheetService.Import(this.Metadata, selectedSheets);
                this.SetDialogResultAction?.Invoke(true);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SubscribeToItem(MusicSheetInfo item)
        {
            item.PropertyChanged -= this.OnMusicSheetInfoPropertyChanged;
            item.PropertyChanged += this.OnMusicSheetInfoPropertyChanged;

            if (item.Parts != null)
            {
                item.Parts.CollectionChanged -= this.OnPartsCollectionChanged;
                item.Parts.CollectionChanged += this.OnPartsCollectionChanged;
            }
        }

        private void CalculateConflicts()
        {
            MusicSheetFolder.CalculateConflicts(this.MusicSheets.Where(m => m.IsSelected).Select(i => i.Sheet).ToList());
        }

        #endregion


        #region Event Handlers

        private void OnMusicSheetsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<MusicSheetInfo>())
                {
                    this.SubscribeToItem(item);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<MusicSheetInfo>())
                {
                    item.PropertyChanged -= this.OnMusicSheetInfoPropertyChanged;
                    if (item.Parts != null)
                    {
                        item.Parts.CollectionChanged -= this.OnPartsCollectionChanged;
                    }
                }
            }

            this.CalculateConflicts();
            
            ((RelayCommand)this.ImportCommand).NotifyCanExecuteChanged();
            this.OnPropertyChanged(nameof(this.IsMetadataReadOnly));
        }

        private void OnMusicSheetInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Relevante Eigenschaften beeinflussen sowohl Konfliktstatus als auch Import-Bedingung
            if (e.PropertyName is 
                nameof(MusicSheetInfo.IsSelected) or 
                nameof(MusicSheetInfo.Instrument) or 
                nameof(MusicSheetInfo.Clef) or 
                nameof(MusicSheetInfo.Parts))
            {
                this.CalculateConflicts();
                ((RelayCommand)this.ImportCommand).NotifyCanExecuteChanged();
            }
        }

        private void OnPartsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.CalculateConflicts();
            ((RelayCommand)this.ImportCommand).NotifyCanExecuteChanged();
        }

        #endregion
    }

    public class SplitOptions : ObservableObject
    {
        #region Fields

        private bool _splitA3ToA4;

        private bool _rotate;

        private PagesPerSheet _pagesPerSheet;

        #endregion


        #region Constructors

        public SplitOptions()
        {
            this.SplitA3ToA4 = true;
            this.Rotate = true;
            this.PagesPerSheet = PagesPerSheet.TwoPages;
        }

        #endregion


        #region Properties

        public bool SplitA3ToA4
        {
            get => _splitA3ToA4;
            set => this.SetProperty(ref _splitA3ToA4, value);
        }

        public bool Rotate
        {
            get => _rotate;
            set => this.SetProperty(ref _rotate, value);
        }

        public PagesPerSheet PagesPerSheet
        {
            get => _pagesPerSheet;
            set => this.SetProperty(ref _pagesPerSheet, value);
        }

        #endregion
    }

    public enum PagesPerSheet
    {
        [System.ComponentModel.DataAnnotations.Display(Name = "1 page")]
        OnePage,
        [System.ComponentModel.DataAnnotations.Display(Name = "2 pages")]
        TwoPages,
        [System.ComponentModel.DataAnnotations.Display(Name = "3 pages")]
        ThreePages,
        [System.ComponentModel.DataAnnotations.Display(Name = "4 pages")]
        FourPages,
        [System.ComponentModel.DataAnnotations.Display(Name = "All pages")]
        AllPages
    }
}
