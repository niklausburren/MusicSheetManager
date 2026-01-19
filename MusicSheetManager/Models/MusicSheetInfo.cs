using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MusicSheetManager.Models
{
    // UI-Wrapper um MusicSheet mit Auswahlstatus
    public class MusicSheetInfo : ObservableObject
    {
        #region Fields

        private bool _isSelected = true;

        #endregion


        #region Constructors

        public MusicSheetInfo(MusicSheet sheet)
        {
            this.Sheet = sheet;
            this.Sheet.PropertyChanged += (s, e) => this.OnPropertyChanged(e.PropertyName); 
        }

        #endregion


        #region Properties

        public MusicSheet Sheet { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => this.SetProperty(ref _isSelected, value);
        }

        public InstrumentInfo Instrument
        {
            get => this.Sheet.Instrument;
            set => this.Sheet.Instrument = value;
        }

        public ObservableCollection<PartInfo> Parts
        {
            get => this.Sheet.Parts;
            set => this.Sheet.Parts = value;
        }

        public ClefInfo Clef
        {
            get => this.Sheet.Clef;
            set => this.Sheet.Clef = value;
        }

        public string FileName => this.Sheet.FileName;

        #endregion
    }
}