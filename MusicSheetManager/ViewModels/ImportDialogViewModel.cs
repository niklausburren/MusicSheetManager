using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

        private MusicSheetFolderMetadata _metadata;

        private SplitOptions _splitOptions;

        private IReadOnlyList<string> _fileNames;

        private bool _isSplittingOrDetecting;

        private int _progress;

        private bool _isMetadataEditable;

        private bool _showSplitControls;

        private string _splitAndDetectButtonText = "Split & detect";

        private string _importTitle = "Import music sheets";

        private string _importDescription;

        private bool _isMetadataExpanded = true;

        private bool _isSplitOptionsExpanded = true;

        #endregion


        #region Constructors

        public ImportDialogViewModel(IMusicSheetService importService)
        {
            _musicSheetService = importService ?? throw new ArgumentNullException(nameof(importService));

            this.Metadata = new MusicSheetFolderMetadata();

            this.SplitOptions = new SplitOptions();
            this.SplitAndDetectCommand = new AsyncRelayCommand(this.ExecuteSplitAndDetectAsync, this.CanSplitMusicSheets);
            this.ImportCommand = new RelayCommand(this.ImportMusicSheets, this.CanImportMusicSheets);

            this.MusicSheets.CollectionChanged += this.OnMusicSheetsCollectionChanged;

            this.ShowSplitControls = true;
            this.SplitAndDetectButtonText = "Split & detect";
            this.ImportDescription = string.Empty;

            this.IsMetadataExpanded = true;
            this.IsSplitOptionsExpanded = true;
        }

        #endregion


        #region Properties

        public int Progress
        {
            get => _progress;
            set => this.SetProperty(ref _progress, value);
        }

        public IReadOnlyList<string> FileNames
        {
            get => _fileNames;
            set
            {
                if (this.SetProperty(ref _fileNames, value))
                {
                    this.UpdateUi();
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

        public ICommand SplitAndDetectCommand { get; }

        public bool IsSplittingOrDetecting
        {
            get => _isSplittingOrDetecting;
            set => this.SetProperty(ref _isSplittingOrDetecting, value);
        }

        public bool IsMetadataEditable
        {
            get => _isMetadataEditable;
            set => this.SetProperty(ref _isMetadataEditable, value);
        }

        public ObservableCollection<MusicSheetInfo> MusicSheets { get; } = [];

        public ICommand ImportCommand { get; }

        public Action<bool?> SetDialogResultAction { get; set; }

        public bool ShowSplitControls
        {
            get => _showSplitControls;
            set => this.SetProperty(ref _showSplitControls, value);
        }

        public string SplitAndDetectButtonText
        {
            get => _splitAndDetectButtonText;
            set => this.SetProperty(ref _splitAndDetectButtonText, value);
        }

        public string ImportTitle
        {
            get => _importTitle;
            set => this.SetProperty(ref _importTitle, value);
        }

        public string ImportDescription
        {
            get => _importDescription;
            set => this.SetProperty(ref _importDescription, value);
        }

        public bool IsMetadataExpanded
        {
            get => _isMetadataExpanded;
            set => SetProperty(ref _isMetadataExpanded, value);
        }

        public bool IsSplitOptionsExpanded
        {
            get => _isSplitOptionsExpanded;
            set => SetProperty(ref _isSplitOptionsExpanded, value);
        }

        #endregion


        #region Protected Methods

        /// <inheritdoc />
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => this.OnPropertyChanged(e));
                return;
            }

            base.OnPropertyChanged(e);

            if (e.PropertyName is nameof(this.IsSplittingOrDetecting))
            {
                ((RelayCommand)this.ImportCommand).NotifyCanExecuteChanged();
            }
        }

        #endregion


        #region Private Methods

        private void UpdateUi()
        {
            var fileCount = this.FileNames?.Count ?? 0;

            this.ShowSplitControls = fileCount <= 1;
            this.SplitAndDetectButtonText = fileCount <= 1
                ? "Split & detect"
                : "Detect";

            this.ImportTitle = fileCount switch
            {
                1 => "Import single PDF file with multiple sheets",
                > 1 => "Import multiple PDF files with only one sheet",
                _ => "Import music sheets"
            };

            this.ImportDescription = fileCount switch
            {
                1 =>
                    "You have selected a PDF file containing multiple music sheets. Enter first the required metadata and select the appropriate options for splitting the file into individual files for each music sheet. Press then the “Split & detect” button to start splitting and automatic recognition of instruments, parts, and clefs.",
                > 1 =>
                    $"You have selected {fileCount} PDF files with only one music sheet. Enter first the required metadata. Press then the “Detect” button to start automatic recognition of instruments, parts, and clefs.",
                _ => string.Empty
            };

            ((AsyncRelayCommand)this.SplitAndDetectCommand).NotifyCanExecuteChanged();
        }

        private bool CanSplitMusicSheets()
        {
            return true;
            // Enable when there is at least one valid file
            if (this.FileNames is not [not null])
            {
                return false;
            }

            // Ensure all entries exist and are PDFs
            return this.FileNames.Count >= 1 &&
                   this.FileNames.All(f => !string.IsNullOrWhiteSpace(f) &&
                                           File.Exists(f) &&
                                           Path.GetExtension(f).Equals(".pdf", StringComparison.OrdinalIgnoreCase));
        }

        private async Task ExecuteSplitAndDetectAsync()
        {
            try
            {
                var fileCount = this.FileNames?.Count ?? 0;

                if (fileCount <= 1)
                {
                    await this.SplitAndDetectMusicSheetsAsync();
                }
                else
                {
                    await this.DetectMusicSheetsAsync();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    Application.Current.MainWindow!,
                    $"Error while splitting or detecting music sheets: {e.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task SplitAndDetectMusicSheetsAsync()
        {
            if (this.EnsureRequiredMetadata())
            {
                return;
            }

            this.IsSplittingOrDetecting = true;

            this.IsMetadataExpanded = false;
            this.IsSplitOptionsExpanded = false;
            this.IsMetadataEditable = false;

            try
            {
                this.Progress = 0;
                this.MusicSheets.Clear();

                var progress = new Progress<(MusicSheet, int)>(report =>
                {
                    var (musicSheet, totalSheets) = report;
                    var info = new MusicSheetInfo(musicSheet);
                    this.MusicSheets.Add(info);
                    this.Progress = this.MusicSheets.Count * 100 / totalSheets;
                });

                await _musicSheetService.SplitAndDetectSheetsAsync(this.FileNames[0], this.Metadata, this.SplitOptions, progress);
            }
            finally
            {
                this.IsSplittingOrDetecting = false;
            }
        }

        private async Task DetectMusicSheetsAsync()
        {
            if (this.EnsureRequiredMetadata())
            {
                return;
            }

            this.IsSplittingOrDetecting = true;

            this.IsMetadataExpanded = false;
            this.IsSplitOptionsExpanded = false;
            this.IsMetadataEditable = false;

            try
            {
                this.Progress = 0;
                this.MusicSheets.Clear();

                var progress = new Progress<(MusicSheet, int)>(report =>
                {
                    var (musicSheet, totalSheets) = report;
                    var info = new MusicSheetInfo(musicSheet);
                    this.MusicSheets.Add(info);
                    this.Progress = this.MusicSheets.Count * 100 / totalSheets;
                });

                await _musicSheetService.DetectSheetsAsync(this.FileNames, this.Metadata, progress);
            }
            finally
            {
                this.IsSplittingOrDetecting = false;
            }
        }

        private bool CanImportMusicSheets()
        {
            var selectedMusicSheets = this.MusicSheets.Where(msi => msi.IsSelected).ToList();

            return !this.IsSplittingOrDetecting && selectedMusicSheets.Any() && selectedMusicSheets.All(msi => msi.Instrument != InstrumentInfo.Unknown && !msi.Sheet.HasConflict);
        }

        private void ImportMusicSheets()
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

        private bool EnsureRequiredMetadata()
        {
            if (string.IsNullOrWhiteSpace(this.Metadata.Title))
            {
                MessageBox.Show(
                    "Please enter a \"Title\" under Metadata.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return true;
            }

            if (string.IsNullOrWhiteSpace(this.Metadata.Composer))
            {
                MessageBox.Show(
                    "Please enter a \"Composer\" under Metadata.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return true;
            }

            return false;
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
        }

        private void OnMusicSheetInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is not (
                nameof(MusicSheetInfo.IsSelected) or
                nameof(MusicSheetInfo.Instrument) or
                nameof(MusicSheetInfo.Clef) or
                nameof(MusicSheetInfo.Parts)))
            {
                return;
            }

            this.CalculateConflicts();
            ((RelayCommand)this.ImportCommand).NotifyCanExecuteChanged();
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
        [Display(Name = "1 page")]
        OnePage,
        [Display(Name = "2 pages")]
        TwoPages,
        [Display(Name = "3 pages")]
        ThreePages,
        [Display(Name = "4 pages")]
        FourPages,
        [Display(Name = "All pages")]
        AllPages
    }
}
