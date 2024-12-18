using System;
using System.Collections.ObjectModel;
using System.IO;
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

        private ObservableCollection<MusicSheet> _musicSheets;

        #endregion


        #region Constructors

        public ImportDialogViewModel(IMusicSheetService importService)
        {
            _musicSheetService = importService ?? throw new ArgumentNullException(nameof(importService));

            this.Metadata = new MusicSheetFolderMetadata();
            this.SplitOptions = new SplitOptions();
            this.SplitCommand = new AsyncRelayCommand(this.ExecuteSplitAsync, this.CanExecuteSplit);
            this.MusicSheets = new ObservableCollection<MusicSheet>();
            this.ImportCommand = new RelayCommand(this.ExecuteImport, this.CanExecuteImport);
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

        public ObservableCollection<MusicSheet> MusicSheets
        {
            get => _musicSheets;
            set => this.SetProperty(ref _musicSheets, value);
        }

        public ICommand ImportCommand { get; }

        public Action<bool?> SetDialogResultAction { get; set; }

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
            this.IsSplitting = true;
            try
            {
                this.Progress = 0;
                this.MusicSheets.Clear();

                var progress = new Progress<(MusicSheet, int)>(report =>
                {
                    var (musicSheet, totalSheets) = report;
                    this.MusicSheets.Add(musicSheet);
                    this.Progress = this.MusicSheets.Count * 100 / totalSheets;
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
            return true;
        }

        private void ExecuteImport()
        {
            try
            {
                _musicSheetService.Import(this.Metadata, this.MusicSheets);
                this.SetDialogResultAction?.Invoke(true);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        OnePage,
        TwoPages,
        ThreePages,
        FourPages,
        AllPages
    }
}
