using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using MusicSheetManager.Converters;
using MusicSheetManager.Editors;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace MusicSheetManager.Models
{
    /// <summary>
    /// Represents a person in the orchestra.
    /// </summary>
    public class Person : ObservableObject
    {
        #region Fields

        private string _firstName;

        private string _lastName;

        private InstrumentInfo _instrument;

        private PartInfo _part;

        private ClefInfo _clef;

        private bool _dispensed;

        #endregion


        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Person"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the person.</param>
        /// <param name="firstName">The first name of the person.</param>
        /// <param name="lastName">The last name of the person.</param>
        /// <param name="instrument">The instrument associated with the person.</param>
        /// <param name="part">The part associated with the person.</param>
        /// <param name="clef">The clef associated with the person.</param>
        [JsonConstructor]
        public Person(Guid id, string firstName, string lastName, InstrumentInfo instrument, PartInfo part, ClefInfo clef)
        {
            this.Id = id;
            _firstName = firstName;
            _lastName = lastName;
            _instrument = instrument;
            _part = part;
            _clef = clef;
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the unique identifier for the person.
        /// </summary>
        [PropertyOrder(1)]
        [ReadOnly(true)]
        public Guid Id { get; }

        /// <summary>
        /// Gets the first name of the person.
        /// </summary>
        [DisplayName("Firstname")]
        [PropertyOrder(2)]
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (this.SetProperty(ref _firstName, value))
                {
                    this.OnPropertyChanged(nameof(this.FullName));
                }
            }
        }

        /// <summary>
        /// Gets the last name of the person.
        /// </summary>
        [DisplayName("Lastname")]
        [PropertyOrder(3)]
        public string LastName
        {
            get => _lastName;
            set
            {
                if (this.SetProperty(ref _lastName, value))
                {
                    this.OnPropertyChanged(nameof(this.FullName));
                }
            }
        }

        /// <summary>
        /// Gets the instrument associated with the person.
        /// </summary>
        [JsonConverter(typeof(InstrumentInfoConverter))]
        [ItemsSource(typeof(InstrumentItemsSource))]
        [PropertyOrder(4)]
        public InstrumentInfo Instrument
        {
            get => _instrument;
            set => this.SetProperty(ref _instrument, value);
        }

        /// <summary>
        /// Gets the parts associated with the person.
        /// </summary>
        [JsonConverter(typeof(PartInfoConverter))]
        [ItemsSource(typeof(PartItemsSource))]
        [PropertyOrder(5)]
        public PartInfo Part
        {
            get => _part;
            set => this.SetProperty(ref _part, value);
        }

        /// <summary>
        /// Gets the clef associated with the person.
        /// </summary>
        [JsonConverter(typeof(ClefInfoConverter))]
        [ItemsSource(typeof(ClefItemsSource))]
        [PropertyOrder(6)]
        public ClefInfo Clef
        {
            get => _clef;
            set => this.SetProperty(ref _clef, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the person is dispensed.
        /// </summary>
        [PropertyOrder(7)]
        public bool Dispensed
        {
            get => _dispensed;
            set => this.SetProperty(ref _dispensed, value);
        }

        /// <summary>
        /// Gets the full name of the person.
        /// </summary>
        [Browsable(false)]
        [JsonIgnore]
        public string FullName => $"{this.LastName} {this.FirstName}";

        #endregion


        #region Public Methods

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.FirstName} {this.LastName}";
        }

        #endregion
    }
}
