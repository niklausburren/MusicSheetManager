using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IronOcr;
using MusicSheetManager.Models;
using MusicSheetManager.Properties;
using MusicSheetManager.Utilities;
using MusicSheetManager.ViewModels;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace MusicSheetManager.Services;

internal class MusicSheetService : IMusicSheetService
{
    #region Constructors

    public MusicSheetService()
    {
        License.LicenseKey = Settings.Default.IronOcrLicenseKey;
    }

    #endregion


    #region IMusicSheetService Members

    /// <inheritdoc />
    public ObservableCollection<MusicSheetFolder> MusicSheetFolders { get; } = [];

    /// <inheritdoc />
    public async Task LoadAsync(IProgress<int> progress)
    {
        this.MusicSheetFolders.Clear();

        var folders = Directory.GetDirectories(Folders.MusicSheetFolder);

        if (folders.Length == 0)
        {
            progress?.Report(100);
            return;
        }

        progress?.Report(0);

        var processed = 0;

        foreach (var folder in folders)
        {
            var musicSheetFolder = await Task.Run(() => MusicSheetFolder.TryLoad(folder));

            if (musicSheetFolder != null)
            {
                this.MusicSheetFolders.Add(musicSheetFolder);
            }
            else
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (Exception)
                {
                    // Delete failed.
                }
            }

            processed++;
            var percent = (int)Math.Round(processed * 100.0 / folders.Length, MidpointRounding.AwayFromZero);
            progress?.Report(percent);
        }
    }

    public async Task SplitAsync(string fileName, MusicSheetFolderMetadata metadata, SplitOptions splitOptions, IProgress<(MusicSheet, int)> progress)
    {
        // Create a temporary folder
        var tempFolder = Path.Combine(Path.GetTempPath(), "MusicSheetManager", "Import");

        if (Directory.Exists(tempFolder))
        {
            Directory.Delete(tempFolder, true);
        }

        Directory.CreateDirectory(tempFolder);

        // Copy the file to the temporary folder
        var tempFilePath = Path.Combine(tempFolder, "original.pdf");
        File.Copy(fileName, tempFilePath);

        if (splitOptions.SplitA3ToA4)
        {
            this.SplitPagesFromA3ToA4(tempFilePath);
        }

        if (splitOptions.Rotate)
        {
            this.RotatePages(tempFilePath);
        }

        using var document = PdfReader.Open(tempFilePath, PdfDocumentOpenMode.Import);

        var pagePerSheetCount = splitOptions.PagesPerSheet.ToInt() ?? document.PageCount;
        var totalSheets = document.PageCount / pagePerSheetCount;

        for (var index = 0; index < document.Pages.Count; index += pagePerSheetCount)
        {
            var musicSheet = await MusicSheet.CreateAsync(metadata, document, index, pagePerSheetCount).ConfigureAwait(false);
            progress.Report((musicSheet, totalSheets));
        }
    }

    /// <inheritdoc />
    public void Import(MusicSheetFolderMetadata metadata, IEnumerable<MusicSheet> musicSheets)
    {
        var sheets = musicSheets.ToList();

        foreach (var sheet in sheets)
        {
            sheet.UpdateFileName(onlyIfNumbered: true);
        }

        var musicSheetFolder = this.MusicSheetFolders.FirstOrDefault(f => f.Title == metadata.Title);

        if (musicSheetFolder == null)
        {
            musicSheetFolder = MusicSheetFolder.Create(metadata);
            this.MusicSheetFolders.Add(musicSheetFolder);
        }

        musicSheetFolder.ImportSheets(sheets);
    }

    public void SplitPagesFromA3ToA4(string fileName)
    {
        using var inputDocument = PdfReader.Open(fileName, PdfDocumentOpenMode.Import);
        var outputDocument = new PdfDocument();

        foreach (var page in inputDocument.Pages)
        {
            double a3Width = page.Width;
            double a3Height = page.Height;

            var a4Height = a3Height / 2;

            var topPage = outputDocument.AddPage(page);
            topPage.CropBox = new PdfRectangle(
                new XPoint(0, a4Height),
                new XPoint(a3Width, a3Height)
            );

            var bottomPage = outputDocument.AddPage(page);
            bottomPage.CropBox = new PdfRectangle(
                new XPoint(0, 0),
                new XPoint(a3Width, a4Height)
            );
        }

        outputDocument.Save(fileName);
    }

    public void RotatePages(string fileName)
    {
        using var document = PdfReader.Open(fileName, PdfDocumentOpenMode.Import);

        foreach (var page in document.Pages)
        {
            var rotation = page.Rotate;
            page.Rotate = (rotation - 90) % 360;
        }

        document.Save(fileName);
    }

    #endregion
}

internal static class PagesPerSheetExtensions
{
    #region Public Methods

    public static int? ToInt(this PagesPerSheet pagesPerSheet)
    {
        return pagesPerSheet switch
        {
            PagesPerSheet.OnePage => 1,
            PagesPerSheet.TwoPages => 2,
            PagesPerSheet.ThreePages => 3,
            PagesPerSheet.FourPages => 4,
            PagesPerSheet.AllPages => null,
            _ => throw new ArgumentOutOfRangeException(nameof(pagesPerSheet), pagesPerSheet, null)
        };
    }

    #endregion
}