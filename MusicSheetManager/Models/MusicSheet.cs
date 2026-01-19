using CommunityToolkit.Mvvm.ComponentModel;
using IronOcr;
using MusicSheetManager.Editors;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace MusicSheetManager.Models;

public class MusicSheet : ObservableObject
{
    #region Fields

    private InstrumentInfo _instrument;

    private ObservableCollection<PartInfo> _parts;

    private ClefInfo _clef;

    private bool _hasConflict;

    #endregion


    #region Constructors

    private MusicSheet(string fileName, MusicSheetFolderMetadata metadata, InstrumentInfo instrument, IEnumerable<PartInfo> parts, ClefInfo clef)
    {
        this.Id = Guid.NewGuid();
        this.FileName = fileName;
        this.Metadata = metadata;

        this.Metadata.PropertyChanged += (_, _) => this.OnPropertyChanged(nameof(this.Metadata));

        _instrument = instrument;
        _parts = new ObservableCollection<PartInfo>(parts);
        _parts.CollectionChanged += (_, _) => this.OnPropertyChanged(nameof(this.Parts));
        _clef = clef;

        this.SaveMetadata();
    }

    private MusicSheet(string fileName)
    {
        this.FileName = fileName;
        this.Metadata = new MusicSheetFolderMetadata();
        this.LoadMetadata();
    }

    #endregion


    #region Properties

    [PropertyOrder(1)]
    public Guid Id { get; private set; }

    [Browsable(false)]
    public Guid FolderId { get; set; }

    [Browsable(false)]
    public string FileName { get; private set; }

    [Browsable(false)]
    public MusicSheetFolderMetadata Metadata { get; }

    [Browsable(false)]
    public string Title
    {
        get => this.Metadata.Title;
        set
        {
            if (this.Metadata.Title == value)
            {
                return;
            }

            this.Metadata.Title = value;
            this.OnPropertyChanged();
        }
    }

    [Browsable(false)]
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
            this.OnPropertyChanged();
        }
    }

    [Browsable(false)]
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
            this.OnPropertyChanged();
        }
    }

    [PropertyOrder(2)]
    [ItemsSource(typeof(InstrumentItemsSource))]
    public InstrumentInfo Instrument
    {
        get => _instrument;
        set
        {
            if (this.SetProperty(ref _instrument, value))
            {
                this.OnPropertyChanged(nameof(this.DisplayName));
            }
        }
    }

    [PropertyOrder(3)]
    [Editor(typeof(PartsEditor), typeof(ITypeEditor))]
    public ObservableCollection<PartInfo> Parts
    {
        get => _parts;
        set
        {
            if (this.SetProperty(ref _parts, value))
            {
                this.OnPropertyChanged(nameof(this.DisplayName));
            }
        }
    }

    [PropertyOrder(4)]
    [ItemsSource(typeof(ClefItemsSource))]
    public ClefInfo Clef
    {
        get => _clef;
        set
        {
            if (this.SetProperty(ref _clef, value))
            {
                this.OnPropertyChanged(nameof(this.DisplayName));
            }
        }
    }

    [Browsable(false)]
    public string DisplayName => this.ToString();

    [Browsable(false)]
    public bool HasConflict
    {
        get => _hasConflict;
        set => this.SetProperty(ref _hasConflict, value);
    }

    #endregion


    #region Public Methods

    public static async Task<MusicSheet> CreateAsync(MusicSheetFolderMetadata metadata, PdfDocument document, int startIndex, int pageCount)
    {
        var ocrResult = await PerformOcrAsync(document, startIndex);

        var fileName = BuildFilename(Path.GetDirectoryName(document.FullPath), metadata.Title, ocrResult.instrument, ocrResult.parts, ocrResult.clef);

        using (var outputDocument = new PdfDocument())
        {
            for (var pageIndex = startIndex; pageIndex < Math.Min(startIndex + pageCount, document.Pages.Count); pageIndex++)
            {
                var exportPage = document.Pages[pageIndex];
                outputDocument.AddPage(exportPage);
            }

            outputDocument.Save(fileName);
            outputDocument.Close();
        }

        return new MusicSheet(fileName, metadata, ocrResult.instrument, ocrResult.parts, ocrResult.clef);
    }

    public static MusicSheet Load(string fileName)
    {
        return new MusicSheet(fileName);
    }

    public void MoveToFolder(string folder)
    {
        this.SaveMetadata();
        
        var newFileName = BuildFilename(folder, this.Title, this.Instrument, this.Parts, this.Clef, false);

        if (File.Exists(newFileName))
        {
            var result = MessageBox.Show(
                Application.Current.MainWindow!,
                $"The file \"{Path.GetFileName(newFileName)}\" already exists. Do you want to replace it?",
                "File Exists",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                return;
            }   
        }
        
        File.Move(this.FileName, newFileName!, true);
        this.FileName = newFileName;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var str = this.Instrument.DisplayName;

        if (this.Parts.Any())
        {
            str += $" - {string.Join(" & ", this.Parts.OrderBy(p => p.Index).Select(p => p.DisplayName))}";
        }

        str += $" - {this.Clef.DisplayName}";

        return str;
    }

    public void UpdateFolder(string newFolder)
    {
        this.FileName = Path.Combine(newFolder, Path.GetFileName(this.FileName)!);
    }

    public void UpdateFileName(bool onlyIfNumbered = false)
    {
        if (onlyIfNumbered && !HasNumberedSuffix(this.FileName))
        {
            return;
        }

        var newFileName = BuildFilename(Path.GetDirectoryName(this.FileName), this.Title, this.Instrument, this.Parts, this.Clef);

        if (newFileName == this.FileName)
        {
            return;
        }

        File.Move(this.FileName, newFileName);
        this.FileName = newFileName;
    }

    #endregion


    #region Protected Methods

    /// <inheritdoc />
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName != nameof(this.Title) &&
            e.PropertyName != nameof(this.Composer) &&
            e.PropertyName != nameof(this.Arranger) &&
            e.PropertyName != nameof(this.Instrument) && 
            e.PropertyName != nameof(this.Parts) &&
            e.PropertyName != nameof(this.Clef))
        {
            return;
        }

        this.SaveMetadata();

        if (e.PropertyName != nameof(this.Title) &&
            e.PropertyName != nameof(this.Instrument) &&
            e.PropertyName != nameof(this.Parts) &&
            e.PropertyName != nameof(this.Clef))
        {
            return;
        }

        this.OnPropertyChanged(nameof(this.DisplayName));
        this.UpdateFileName();
    }

    #endregion


    #region Private Methods

    private static async Task<(InstrumentInfo instrument, IReadOnlyList<PartInfo> parts, ClefInfo clef)> PerformOcrAsync(PdfDocument document, int startIndex)
    {
        var languages = new[] { OcrLanguage.EnglishBest, OcrLanguage.GermanBest };

        var instrument = InstrumentInfo.Unknown;
        var parts = new List<PartInfo>();
        var clef = ClefInfo.TrebleClef;

        foreach (var language in languages)
        {
            var ocr = new IronTesseract();
            using var ocrInput = new OcrInput();
            ocr.Language = language;

            var contentArea = new System.Drawing.Rectangle(0, 0, 700, 300);
            ocrInput.LoadPdfPage(document.FullPath, startIndex, 200, false, contentArea);
            var ocrResult = await ocr.ReadAsync(ocrInput).ConfigureAwait(false);

            instrument = InstrumentInfo.TryGet(ocrResult.Text);
            parts = PartInfo.TryGet(ocrResult.Text, instrument).ToList();
            clef = ClefInfo.TryGet(ocrResult.Text, instrument);

            if (instrument != InstrumentInfo.Unknown)
            {
                return (instrument, parts, clef);
            }
        }

        return (instrument, parts, clef);
    }

    private static string BuildFilename(string folder, string name, InstrumentInfo instrument, IReadOnlyList<PartInfo> parts, ClefInfo clef, bool unique = true)
    {
        var fileName = $"{name} - {instrument.DisplayName}";

        if (parts.Any())
        {
            fileName += $" - {string.Join(" & ", parts.OrderBy(p => p.Index).Select(v => v.DisplayName))}";
        }

        fileName += $" - {clef.DisplayName}";

        if (!unique)
        {
            return Path.Combine(folder, $"{fileName}.pdf");
        }

        var uniqueFileName = fileName;
        var index = 1;

        while (File.Exists(Path.Combine(folder, $"{uniqueFileName}.pdf")))
        {
            uniqueFileName = $"{fileName} ({index++})";
        }

        return Path.Combine(folder, $"{uniqueFileName}.pdf");
    }

    private void SaveMetadata()
    {
        using var document = PdfReader.Open(this.FileName, PdfDocumentOpenMode.Modify);

        document.SetInfoElement(nameof(this.Id), this.Id.ToString());
        document.SetInfoElement(nameof(this.FolderId), this.FolderId.ToString());
        document.SetInfoElement(nameof(this.Title), this.Title);
        document.SetInfoElement(nameof(this.Composer), this.Composer);
        document.SetInfoElement(nameof(this.Arranger), this.Arranger);
        document.SetInfoElement(nameof(this.Instrument), this.Instrument.Key);
        document.SetInfoElement(nameof(this.Parts), string.Join(";", this.Parts.Select(v => v.Key)));
        document.SetInfoElement(nameof(this.Clef), this.Clef.Key);

        document.Save(this.FileName);
    }

    private void LoadMetadata()
    {
        using var document = PdfReader.Open(this.FileName, PdfDocumentOpenMode.ReadOnly);

        this.Id = Guid.Parse(document.GetInfoElement(nameof(this.Id)));
        this.FolderId = Guid.Parse(document.GetInfoElement(nameof(this.FolderId)));
        this.Metadata.Title = document.GetInfoElement(nameof(this.Title));
        this.Metadata.Composer = document.GetInfoElement(nameof(this.Composer));
        this.Metadata.Arranger = document.GetInfoElement(nameof(this.Arranger));
        _instrument = InstrumentInfo.GetByKey(document.GetInfoElement(nameof(this.Instrument)));

        var partKeysCsv = document.GetInfoElement(nameof(this.Parts));
        _parts = new ObservableCollection<PartInfo>(!string.IsNullOrEmpty(partKeysCsv) ? partKeysCsv.Split(';').Select(PartInfo.GetByKey) : []);
        _parts.CollectionChanged += (_, _) => this.OnPropertyChanged(nameof(this.Parts));

        _clef = ClefInfo.GetByKey(document.GetInfoElement(nameof(this.Clef)));
    }

    private static bool HasNumberedSuffix(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        return !string.IsNullOrEmpty(name) && Regex.IsMatch(name, @"\s\(\d+\)$");
    }

    #endregion
}

internal static class PdfDocumentExtensions
{
    #region Constants

    private const string KEY_PREFIX = "MusicSheetManager";

    #endregion


    #region Public Methods

    public static void SetInfoElement(this PdfDocument document, string key, string value)
    {
        document.Info.Elements[$"/{KEY_PREFIX}_{key}"] = new PdfString(value);
    }

    public static string GetInfoElement(this PdfDocument document, string key)
    {
        var value = document.Info.Elements[$"/{KEY_PREFIX}_{key}"]?.ToString()?.Trim('(', ')');
        return value;
    }

    #endregion
}
