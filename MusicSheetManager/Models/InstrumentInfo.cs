using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using MusicSheetManager.Properties;

// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo

namespace MusicSheetManager.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class InstrumentInfo : IEquatable<InstrumentInfo>
{
    #region Constructors

    private InstrumentInfo(string key, InstrumentCategory category, params string[] patterns)
    {
        this.Key = key;
        this.Category = category;
        this.Patterns = patterns;
    }

    #endregion


    #region Properties

    public static InstrumentInfo Unknown { get; } = new("Unknown", InstrumentCategory.Unknown, "Unknown");

    public static InstrumentInfo Score { get; } = new("Score", InstrumentCategory.Conductor, "Score");

    public static InstrumentInfo Flute { get; } = new("Flute", InstrumentCategory.Wood, "C Flute", "Flute", "Flöte C", "Flöte");

    public static InstrumentInfo Piccolo { get; } = new("Piccolo", InstrumentCategory.Wood, "Piccolo");

    public static InstrumentInfo Oboe { get; } = new("Oboe", InstrumentCategory.Wood, "Oboe");

    public static InstrumentInfo Bassoon { get; } = new("Bassoon", InstrumentCategory.Wood, "Bassoon", "Fagott");

    public static InstrumentInfo AltoClarinetBb { get; } = new("AltoClarinetBb", InstrumentCategory.Wood, "Eb Alto Clarinet", "Alto Clarinet", "Klarinette Alt B", "Klarinette Alt");

    public static InstrumentInfo BassClarinetBb { get; } = new("BassClarinetBb", InstrumentCategory.Wood, "Bb Bass Clarinet", "Bass Clarinet in B", "Bass Clarinet", "Bass Klarinette B", "Bass Klarinette");

    public static InstrumentInfo ClarinetEb { get; } = new("ClarinetEb", InstrumentCategory.Wood, "Eb Clarinet", "Clarinet .{0,6}in Eb", "Klarinette Es");

    public static InstrumentInfo ClarinetBb { get; } = new("ClarinetBb", InstrumentCategory.Wood, "Bb Clarinet", "Clarinet .{0,6}in B", "Clarinet", "Klarinette B", "Klarinette");

    public static InstrumentInfo SopranoSaxophoneBb { get; } = new("SopranoSaxophoneBb", InstrumentCategory.Wood, "Bb Soprano Saxophone", "Soprano Saxophon B", "Saxophon soprano in B");

    public static InstrumentInfo AltoSaxophoneEb { get; } = new("AltoSaxophoneEb", InstrumentCategory.Wood, "Eb Alto Saxophone", "Alto Saxophon .{0,6}in Eb", "Alto Saxophone", "Saxophon Alt Es");

    public static InstrumentInfo TenorSaxophoneBb { get; } = new("TenorSaxophoneBb", InstrumentCategory.Wood, "Bb Tenor Saxophone", "Tenor Saxophon in B", "Tenor Saxophone", "Saxophon Tenor B");

    public static InstrumentInfo BaritoneSaxophoneEb { get; } = new("BaritoneSaxophoneEb", InstrumentCategory.Wood, "Eb Baritone Saxophone", "Bariton Saxophon Eb", "Baritone Saxophone", "Saxophon Bariton Es");

    public static InstrumentInfo CornetBb { get; } = new("CornetBb", InstrumentCategory.Brass, "Bb Cornet", "Cornet B", "Cornet");

    public static InstrumentInfo FlugelhornBb { get; } = new("FlugelhornBb", InstrumentCategory.Brass, "Bb Flugelhorn", "Flugelhorn .{0,6}in B", "Flügelhorn B", "Flügelhorn");

    public static InstrumentInfo TrumpetBb { get; } = new("TrumpetBb", InstrumentCategory.Brass, "Bb Trumpet", "Trumpet .{0,6}in B", "Trumpet", "Trompete B", "Trompete");

    public static InstrumentInfo HornF { get; } = new("HornF", InstrumentCategory.Brass, "F Horn", "Horn .{0,6}in F", "Horn F");

    public static InstrumentInfo HornEb { get; } = new("HornEb", InstrumentCategory.Brass, "Eb Horn", "Horn .{0,6}in Eb", "Horn Es");

    public static InstrumentInfo BassTromboneBb { get; } = new("BassTromboneBb", InstrumentCategory.Brass, "Bb Bass Trombone", "Bass Trombone", "Bass Posaune");

    public static InstrumentInfo TromboneBb { get; } = new("TromboneBb", InstrumentCategory.Brass, "Bb Trombone", "Trombone .{0,6}in Bb", "C Trombone", "Trombone .{0,6}in C", "Trombone", "Posaune B", "Posaune");

    public static InstrumentInfo BaritoneHornBb { get; } = new("BaritoneHornBb", InstrumentCategory.Brass, "Bb Baritone Horn", "Baritone in Bb", "Bariton B", "Bariton 5", "C Baritone Horn", "Baritone in C", "Bariton C", "Bariton");

    public static InstrumentInfo TenorHornEb { get; } = new("TenorHornEb", InstrumentCategory.Brass, "Eb Tenor Horn", "Althorn Es");

    public static InstrumentInfo TenorHornBb { get; } = new("TenorHornBb", InstrumentCategory.Brass, "Bb Tenor Horn", "Tenorhorn B");

    public static InstrumentInfo EuphoniumBb { get; } = new("EuphoniumBb", InstrumentCategory.Brass, "Bb Euphonium", "Euphonium in Bb", "C Euphonium", "Euphonium in C", "Euphonium C", "Euphonium");

    public static InstrumentInfo BassEb { get; } = new("BassEb", InstrumentCategory.Brass, "Eb Bass", "Bass in Eb", "Bass Es", "Bässe .{0,6}Eb");

    public static InstrumentInfo BassBb { get; } = new("BassBb", InstrumentCategory.Brass, "Bb Bass", "Bass in Bb", "Bass B", "Bässe .{0,6}Bb", "C Bass", "Bass C", "Bässe .{0,6}C", "Bb Tuba", "Tuba");

    public static InstrumentInfo Basses { get; } = new("Basses", InstrumentCategory.Brass, "Basses");

    public static InstrumentInfo StringBass { get; } = new("StringBass", InstrumentCategory.Strings, "String Bass", "Basses", "Kontrabass");

    public static InstrumentInfo DrumSet { get; } = new("DrumSet", InstrumentCategory.Percussion, "Drum Set");

    public static InstrumentInfo SnareDrum { get; } = new("SnareDrum", InstrumentCategory.Percussion, "Snare Drum", "Kleine Trommel");

    public static InstrumentInfo BassDrum { get; } = new("BassDrum", InstrumentCategory.Percussion, "Bass Drum", "Grosse Trommel");

    public static InstrumentInfo Timpani { get; } = new("Timpani", InstrumentCategory.Percussion, "Timpani");

    public static InstrumentInfo Mallets { get; } = new("Mallets", InstrumentCategory.Percussion, "Mallets", "Mallet Percussion");

    public static InstrumentInfo Lyra { get; } = new("Lyra", InstrumentCategory.Percussion, "Lyra");

    public static InstrumentInfo Percussion { get; } = new("Percussion", InstrumentCategory.Percussion, "Percussion", "Perkussion");

    public static IEnumerable<InstrumentInfo> All => typeof(InstrumentInfo)
        .GetProperties(BindingFlags.Static | BindingFlags.Public)
        .Select(pi => pi.GetValue(null))
        .OfType<InstrumentInfo>();

    public string Key { get; }

    public int Index => All.ToList().IndexOf(this);

    public InstrumentCategory Category { get; }

    public string[] Patterns { get; }

    public string DisplayName => Resources.ResourceManager.GetString($"{nameof(InstrumentInfo)}_{this.Key}") ?? this.Key;

    #endregion


    #region Public Methods

    /// <summary>
    /// The equality operator.
    /// </summary>
    public static bool operator ==(InstrumentInfo left, InstrumentInfo right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// The inequality operator.
    /// </summary>
    public static bool operator !=(InstrumentInfo left, InstrumentInfo right)
    {
        return !Equals(left, right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.DisplayName;
    }

    public static InstrumentInfo GetByKey(string key)
    {
        return All.Single(clef => clef.Key == key);
    }

    public static InstrumentInfo TryGet(string text)
    {
        var closestMatch = Unknown;
        var closestIndex = int.MaxValue;

        foreach (var instrumentInfo in All)
        {
            foreach (var pattern in instrumentInfo.Patterns)
            {
                var result = Regex.Match(text, pattern, RegexOptions.IgnoreCase);

                if (result.Success && result.Index < closestIndex)
                {
                    closestMatch = instrumentInfo;
                    closestIndex = result.Index;
                }
            }
        }

        return closestMatch;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return this.Equals((InstrumentInfo)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return this.Key != null
            ? this.Key.GetHashCode()
            : 0;
    }

    #endregion


    #region IEquatable<InstrumentInfo> Members

    /// <inheritdoc />
    public bool Equals(InstrumentInfo other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return this.Key == other.Key;
    }

    #endregion
}

public enum InstrumentCategory
{
    Unknown,
    Conductor,
    Wood,
    Brass,
    Percussion,
    Strings
}

public static class InstrumentInfoExtensions
{
    #region Properties

    private static IDictionary<InstrumentInfo, IEnumerable<InstrumentInfo>> FallbackInstruments { get; } = new Dictionary<InstrumentInfo, IEnumerable<InstrumentInfo>>
    {
        { InstrumentInfo.Piccolo, [InstrumentInfo.Flute] },
        { InstrumentInfo.Flute, [InstrumentInfo.Oboe] },
        { InstrumentInfo.CornetBb, [InstrumentInfo.TrumpetBb, InstrumentInfo.FlugelhornBb ] },
        { InstrumentInfo.TrumpetBb, [InstrumentInfo.CornetBb, InstrumentInfo.FlugelhornBb ] },
        { InstrumentInfo.EuphoniumBb, [InstrumentInfo.BaritoneHornBb, InstrumentInfo.TenorHornBb] },
        { InstrumentInfo.BassClarinetBb, [InstrumentInfo.BassBb] },
        { InstrumentInfo.BassBb, [InstrumentInfo.BassClarinetBb] },
        { InstrumentInfo.Bassoon, [InstrumentInfo.BaritoneHornBb, InstrumentInfo.BassTromboneBb, InstrumentInfo.TromboneBb] },
        { InstrumentInfo.BaritoneSaxophoneEb, [InstrumentInfo.BassEb] },
        { InstrumentInfo.BassEb, [InstrumentInfo.BaritoneSaxophoneEb] },
    };

    #endregion


    #region Public Methods

    public static IEnumerable<InstrumentInfo> GetSelfAndFallbacks(this InstrumentInfo instrument)
    {
        return new[] { instrument }.Concat(instrument.GetFallbacks());
    }

    public static IEnumerable<InstrumentInfo> GetFallbacks(this InstrumentInfo instrument)
    {
        return FallbackInstruments.TryGetValue(instrument, out var fallbacks)
            ? fallbacks
            : Enumerable.Empty<InstrumentInfo>();
    }

    #endregion
}
