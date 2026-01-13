using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using MusicSheetManager.Converters;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace MusicSheetManager.Models
{
    /// <summary>
    /// Represents a person in the orchestra.
    /// </summary>
    public class Person : ObservableObject
    {
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
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Instrument = instrument;
            this.Part = part;
            this.Clef = clef;
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the unique identifier for the person.
        /// </summary>
        [Browsable(false)]
        public Guid Id { get; }

        /// <summary>
        /// Gets the first name of the person.
        /// </summary>
        [DisplayName("Firstname")]
        [PropertyOrder(1)]
        public string FirstName { get; }

        /// <summary>
        /// Gets the last name of the person.
        /// </summary>
        [DisplayName("Lastname")]
        [PropertyOrder(2)]
        public string LastName { get; }

        /// <summary>
        /// Gets the instrument associated with the person.
        /// </summary>
        [JsonConverter(typeof(InstrumentInfoConverter))]
        [PropertyOrder(3)]
        public InstrumentInfo Instrument { get; }

        /// <summary>
        /// Gets the parts associated with the person.
        /// </summary>
        [JsonConverter(typeof(PartInfoConverter))]
        [PropertyOrder(4)]
        public PartInfo Part { get; }

        /// <summary>
        /// Gets the clef associated with the person.
        /// </summary>
        [JsonConverter(typeof(ClefInfoConverter))]
        [PropertyOrder(5)]
        public ClefInfo Clef { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the person is dispensed.
        /// </summary>
        [PropertyOrder(6)]
        public bool Dispensed { get; set; }

        /// <summary>
        /// Gets the full name of the person.
        /// </summary>
        [Browsable(false)]
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
