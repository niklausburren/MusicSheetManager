using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MusicSheetManager.Utilities;

namespace MusicSheetManager.Models
{
    public class MusicSheetFolder
    {
        #region Fields

        private readonly IList<MusicSheet> _sheets = new List<MusicSheet>();

        #endregion


        #region Constructors

        public MusicSheetFolder(MusicSheetFolderMetadata metadata)
        {
            this.Id = Guid.NewGuid();
            this.Metadata = metadata;
            this.Folder = Path.Combine(Folders.MusicSheetFolder, this.Metadata.Title);
        }

        public MusicSheetFolder(string folder)
        {
            this.Folder = folder;

            _sheets = Directory.GetFiles(this.Folder, "*.pdf")
                .Select(MusicSheet.Load)
                .ToList();

            this.Id = this.Sheets[0].FolderId;
            this.Metadata = this.Sheets[0].Metadata;
        }

        #endregion


        #region Properties

        public Guid Id { get; }

        public string Folder { get; }

        private MusicSheetFolderMetadata Metadata { get; }

        public IReadOnlyList<MusicSheet> Sheets => _sheets as IReadOnlyList<MusicSheet>;

        public string Title => this.Metadata.Title;

        public string Composer => this.Metadata.Composer;

        public string Arranger => this.Metadata.Arranger;

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
            return Directory.Exists(folder)
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

            if (sheets.Any(s => MusicSheet.HasNumberingInParentheses(s.FileName)))
            {
                throw new ArgumentException("All music sheets must have a unique file name. The wrong instrument, part or clef has been selected for at least one music sheet.");
            }

            if (!Directory.Exists(this.Folder))
            {
                Directory.CreateDirectory(this.Folder);
            }

            foreach (var sheet in sheets)
            {
                sheet.FolderId = this.Id;
                sheet.MoveToFolder(this.Folder);
                _sheets.Add(sheet);
            }
        }

        #endregion
    }
}
