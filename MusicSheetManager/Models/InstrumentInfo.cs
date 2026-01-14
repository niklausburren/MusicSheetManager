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

    public static InstrumentInfo EnglishHorn { get; } = new("EnglishHorn", InstrumentCategory.Wood, "English Horn", "Englischhorn");

    public static InstrumentInfo Bassoon { get; } = new("Bassoon", InstrumentCategory.Wood, "Bassoon", "Fagott");

    public static InstrumentInfo ClarinetBb { get; } = new("ClarinetBb", InstrumentCategory.Wood, "Bb Clarinet", "Clarinet Bb", "Clarinet in Bb", "Clarinet .{0,6}in Bb", "Bb Klarinette", "Klarinette Bb", "Klarinette in Bb", "Klarinette .{0,6}in Bb", "B Klarinette", "Klarinette B", "Klarinette in B", "Klarinette .{0,6}in B");

    public static InstrumentInfo ClarinetEb { get; } = new("ClarinetEb", InstrumentCategory.Wood, "Eb Clarinet", "Clarinet Eb", "Clarinet in Eb", "Clarinet .{0,6}in Eb", "Eb Klarinette", "Klarinette Eb", "Klarinette in Eb", "Klarinette .{0,6}in Eb", "Es Klarinette", "Klarinette Es", "Klarinette in Es", "Klarinette .{0,6}in Es");

    public static InstrumentInfo AltoClarinetEb { get; } = new("AltoClarinetEb", InstrumentCategory.Wood, "Eb Alto Clarinet", "Alto Clarinet Eb", "Alto Clarinet in Eb", "Eb Klarinette", "Klarinette Eb", "Klarinette in Eb", "Klarinette Alt Es", "Klarinette Alt", "Alt Klarinette");

    public static InstrumentInfo BassClarinetBb { get; } = new("BassClarinetBb", InstrumentCategory.Wood, "Bb Bass Clarinet", "Bass Clarinet Bb", "Bass Clarinet in Bb", "Bass Clarinet", "B Bass Klarinette", "Bass Klarinette B", "Bass Klarinette in B", "Bass Klarinette", "B Bassklarinette", "Bassklarinette B", "Bassklarinette in B", "Bassklarinette");

    public static InstrumentInfo ContrabassClarinetBb { get; } = new("ContrabassClarinetBb", InstrumentCategory.Wood, "Bb Contrabass Clarinet", "Contrabass Clarinet Bb", "Contrabass Clarinet in Bb", "Contrabass Clarinet", "B Kontrabass Klarinette", "Kontrabass Klarinette B", "Kontrabass Klarinette in B", "Kontrabass Klarinette", "B Kontrabassklarinette", "Kontrabassklarinette B", "Kontrabassklarinette in B", "Kontrabassklarinette");

    public static InstrumentInfo SopranoSaxophoneBb { get; } = new("SopranoSaxophoneBb", InstrumentCategory.Wood, "Bb Soprano Saxophone", "Soprano Saxophon B", "Saxophon soprano in B");

    public static InstrumentInfo AltoSaxophoneEb { get; } = new("AltoSaxophoneEb", InstrumentCategory.Wood, "Eb Alto Saxophone", "Alto Saxophone Eb", "Alto Saxophone in Eb", "Alto Saxophon .{0,6}in Eb", "Alto Saxophone", "Eb Altsaxophon", "Altsaxophon Eb", "Altsaxophon in Eb", "Altsaxophon .{0,6}in Eb", "Altsaxophon", "Saxophon Alt Es");

    public static InstrumentInfo TenorSaxophoneBb { get; } = new("TenorSaxophoneBb", InstrumentCategory.Wood, "Bb Tenor Saxophone", "Tenor Saxophon in Bb", "Tenor Saxophon .{0,6}in Bb", "Tenor Saxophone", "Bb Tenorsaxophon", "Tenorsaxophon Bb", "Tenorsaxophon in Bb", "Tenorsaxophon .{0,6}in Bb", "Tenorsaxophon", "Saxophon Tenor B");

    public static InstrumentInfo BaritoneSaxophoneEb { get; } = new("BaritoneSaxophoneEb", InstrumentCategory.Wood, "Eb Baritone Saxophone", "Bariton Saxophon Eb", "Bariton Saxophon in Eb", "Baritone Saxophone", "Eb Baritonsaxophon", "Baritonsaxophon Eb", "Baritonsaxophon in Eb", "Baritonsaxophon", "Saxophon Bariton Es");

    public static InstrumentInfo CornetBb { get; } = new("CornetBb", InstrumentCategory.Brass, "Bb Cornet", "Cornet Bb", "Cornet in Bb", "Cornet .{0,6}in Bb", "B Cornet", "Cornet B", "Cornet in B", "Cornet .{0,6}in B");

    public static InstrumentInfo CornetEb { get; } = new("CornetEb", InstrumentCategory.Brass, "Eb Cornet", "Cornet Eb", "Cornet in Eb", "Cornet .{0,6}in Eb", "Es Cornet", "Cornet Es", "Cornet in Es", "Cornet .{0,6}in Es");

    public static InstrumentInfo TrumpetBb { get; } = new("TrumpetBb", InstrumentCategory.Brass, "Bb Trumpet", "Trumpet .{0,6}in B", "Trumpet", "Trompete B", "Trompete");

    public static InstrumentInfo FlugelhornBb { get; } = new("FlugelhornBb", InstrumentCategory.Brass, "Bb Flugelhorn", "Flugelhorn .{0,6}in B", "Flügelhorn B");

    public static InstrumentInfo FlugelhornEb { get; } = new("FlugelhornEb", InstrumentCategory.Brass, "Eb Flugelhorn", "Flugelhorn .{0,6}in Eb", "Flügelhorn Eb");

    public static InstrumentInfo HornEb { get; } = new("HornEb", InstrumentCategory.Brass, "Eb Horn", "Horn Eb", "Horn in Eb", "Horn .{0,6}in Eb", "Es Horn", "Horn Es", "Horn in Es", "Horn .{0,6}in Es");

    public static InstrumentInfo HornF { get; } = new("HornF", InstrumentCategory.Brass, "F Horn", "Horn F", "Horn in F", "Horn .{0,6}in F");

    public static InstrumentInfo TromboneBb { get; } = new("TromboneBb", InstrumentCategory.Brass, "Bb Trombone", "Trombone Bb", "Trombone in Bb", "Trombone .{0,6}in Bb", "Bb Posaune", "Posaune Bb", "Posaune in Bb", "Posaune .{0,6}in Bb", "B Posaune", "Posaune B", "Posaune in B", "Posaune .{0,6}in B");

    public static InstrumentInfo TromboneC { get; } = new("TromboneC", InstrumentCategory.Brass, "C Trombone", "Trombone C", "Trombone in C", "Trombone .{0,6}in C", "C Posaune", "Posaune C", "Posaune in C", "Posaune .{0,6}in C");

    public static InstrumentInfo BassTromboneBb { get; } = new("BassTromboneBb", InstrumentCategory.Brass, "Bb Bass Trombone", "Bass Trombone Bb", "Bass Trombone in Bb", "Bb Bass Posaune", "Bass Posaune Bb", "Bass Posaune in Bb", "Bb Bassposaune", "Bassposaune Bb", "Bassposaune in Bb");

    public static InstrumentInfo BassTromboneC { get; } = new("BassTromboneC", InstrumentCategory.Brass, "C Bass Trombone", "Bass Trombone C", "Bass Trombone in C", "C Bass Posaune", "Bass Posaune C", "Bass Posaune in C", "C Bassposaune", "Bassposaune C", "Bassposaune in C");

    public static InstrumentInfo BaritoneHornC { get; } = new("BaritoneHornC", InstrumentCategory.Brass, "C Baritone Horn", "Baritone Horn C", "Baritone Horn in C", "C Baritone", "Baritone C", "Baritone in C", "C Bariton", "Bariton C", "Bariton in C");

    public static InstrumentInfo BaritoneHornBb { get; } = new("BaritoneHornBb", InstrumentCategory.Brass, "Bb Baritone Horn", "Baritone Horn Bb", "Baritone Horn in Bb", "Baritone Horn .{0,6}in Bb", "Bb Baritone", "Baritone Bb", "Baritone in Bb", "Baritone .{0,6}in Bb", "Bb Bariton", "Bariton Bb", "Bariton in Bb", "Bariton .{0,6}in Bb", "B Bariton", "Bariton B", "Bariton in B", "Bariton .{0,6}in B");

    public static InstrumentInfo TenorHornBb { get; } = new("TenorHornBb", InstrumentCategory.Brass, "Bb Tenor Horn", "Tenor Horn Bb", "Tenor Horn in Bb", "Tenor Horn .{0,6}in Bb", "Bb Tenorhorn", "Tenorhorn Bb", "Tenorhorn in Bb", "Tenorhorn .{0,6}in Bb", "B Tenorhorn", "Tenorhorn B", "Tenorhorn in B", "Tenorhorn .{0,6}in B");

    public static InstrumentInfo TenorHornC { get; } = new("TenorHornC", InstrumentCategory.Brass, "C Tenor Horn", "Tenor Horn C", "Tenor Horn in C", "Tenor Horn .{0,6}in C", "C Tenorhorn", "Tenorhorn C", "Tenorhorn in C", "Tenorhorn .{0,6}in C");

    public static InstrumentInfo TenorHornEb { get; } = new("TenorHornEb", InstrumentCategory.Brass, "Eb Tenor Horn", "Tenor Horn Eb", "Tenor Horn in Eb", "Tenor Horn .{0,6}in Eb", "Eb Tenorhorn", "Tenorhorn Eb", "Tenorhorn in Eb", "Tenorhorn .{0,6}in Eb", "Althorn Es");

    public static InstrumentInfo EuphoniumBb { get; } = new("EuphoniumBb", InstrumentCategory.Brass, "Bb Euphonium", "Euphonium Bb", "Euphonium in Bb", "Euphonium .{0,6}in Bb");

    public static InstrumentInfo EuphoniumC { get; } = new("EuphoniumC", InstrumentCategory.Brass, "C Euphonium", "Euphonium C", "Euphonium in C", "Euphonium .{0,6}in C");

    public static InstrumentInfo BassBb { get; } = new("BassBb", InstrumentCategory.Brass, "Bb Bass", "Bass Bb", "Bass in Bb", "Bass .{0,6}in Bb", "Bb Bass", "Bass Bb", "Bass in Bb", "Bass .{0,6}in Bb", "B Bass", "Bass B", "Bass in B", "Bass .{0,6}in B", "B Bässe", "Bässe B", "Bässe in B", "Bässe .{0,6}in B", "Bb Tuba", "Tuba Bb", "Tuba in Bb", "Tuba .{0,6}in Bb");

    public static InstrumentInfo BassC { get; } = new("BassC", InstrumentCategory.Brass, "C Bass", "Bass C", "Bass in C", "Bass .{0,6}in C", "C Bässe", "Bässe C", "Bässe in C", "Bässe .{0,6}in C", "C Tuba", "Tuba C", "Tuba in C", "Tuba .{0,6}in C");

    public static InstrumentInfo BassEb { get; } = new("BassEb", InstrumentCategory.Brass, "Eb Bass", "Bass Eb", "Bass in Eb", "Bass .{0,6}in Eb", "Es Bass", "Bass Es", "Bass in Es", "Bass .{0,6}in Es", "Eb Bässe", "Bässe Eb", "Bässe in Eb", "Bässe .{0,6}in Eb", "Eb Tuba", "Tuba Eb", "Tuba in Eb", "Tuba .{0,6}in Eb");

    public static InstrumentInfo Basses { get; } = new("Basses", InstrumentCategory.Brass, "Basses");

    public static InstrumentInfo Alphorn { get; } = new("Alphorn", InstrumentCategory.Brass, "Alphorn", "Alphörner");

    public static InstrumentInfo Percussion { get; } = new("Percussion", InstrumentCategory.Percussion, "Percussion", "Perkussion");

    public static InstrumentInfo DrumSet { get; } = new("DrumSet", InstrumentCategory.Percussion, "Drum Set", "Drums");

    public static InstrumentInfo Cymbals { get; } = new("Cymbals", InstrumentCategory.Percussion, "Cymbals", "Becken");

    public static InstrumentInfo SnareDrum { get; } = new("SnareDrum", InstrumentCategory.Percussion, "Snare Drum", "Kleine Trommel");

    public static InstrumentInfo BassDrum { get; } = new("BassDrum", InstrumentCategory.Percussion, "Bass Drum", "Grosse Trommel");

    public static InstrumentInfo QuadToms { get; } = new("QuadToms", InstrumentCategory.Percussion, "Quad Toms");

    public static InstrumentInfo Timpani { get; } = new("Timpani", InstrumentCategory.Percussion, "Timpani");

    public static InstrumentInfo TubularBells { get; } = new("TubularBells", InstrumentCategory.Percussion, "Tubular Bells", "Chimes", "Röhrenglockenspiel");

    public static InstrumentInfo Glockenspiel { get; } = new("Glockenspiel", InstrumentCategory.Percussion, "Glockenspiel");

    public static InstrumentInfo Mallets { get; } = new("Mallets", InstrumentCategory.Percussion, "Mallets", "Mallet Percussion");

    public static InstrumentInfo Lyra { get; } = new("Lyra", InstrumentCategory.Percussion, "Lyra");

    public static InstrumentInfo Tambourine { get; } = new("Tambourine", InstrumentCategory.Percussion, "Tambourine", "Tamburin");

    public static InstrumentInfo Triangle { get; } = new("Triangle", InstrumentCategory.Percussion, "Triangle", "Triangel");

    public static InstrumentInfo SleighBells { get; } = new("SleighBells", InstrumentCategory.Percussion, "Sleigh Bells", "Schellen");

    public static InstrumentInfo StringBass { get; } = new("StringBass", InstrumentCategory.Strings, "String Bass", "Basses", "Kontrabass");

    public static InstrumentInfo ElectricBass { get; } = new("ElectricBass", InstrumentCategory.Strings, "Electric Bass", "E-Bass");

    public static InstrumentInfo Guitar { get; } = new("Guitar", InstrumentCategory.Strings, "Guitar", "Gitarre");

    public static InstrumentInfo Piano { get; } = new("Piano", InstrumentCategory.Keys, "Piano", "Klavier");

    public static InstrumentInfo Keyboard { get; } = new("Keyboard", InstrumentCategory.Keys, "Keyboard");

    public static IReadOnlyList<InstrumentInfo> All { get; } = typeof(InstrumentInfo)
        .GetProperties(BindingFlags.Static | BindingFlags.Public)
        .Select(pi => pi.GetValue(null))
        .OfType<InstrumentInfo>()
        .ToList();

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
    Strings,
    Keys
}

public static class InstrumentInfoExtensions
{
    #region Properties

    private static IDictionary<InstrumentInfo, IEnumerable<InstrumentInfo>> FallbackInstruments { get; } = new Dictionary<InstrumentInfo, IEnumerable<InstrumentInfo>>
    {
        { InstrumentInfo.Score, [InstrumentInfo.ClarinetBb] },
        { InstrumentInfo.Piccolo, [InstrumentInfo.Flute] },
        { InstrumentInfo.Flute, [InstrumentInfo.Oboe] },
        { InstrumentInfo.Oboe, [InstrumentInfo.Flute] },
        { InstrumentInfo.CornetBb, [InstrumentInfo.TrumpetBb, InstrumentInfo.FlugelhornBb ] },
        { InstrumentInfo.TrumpetBb, [InstrumentInfo.CornetBb, InstrumentInfo.FlugelhornBb ] },
        { InstrumentInfo.EuphoniumBb, [InstrumentInfo.TenorHornBb, InstrumentInfo.BaritoneHornBb] },
        { InstrumentInfo.BassClarinetBb, [InstrumentInfo.BassBb, InstrumentInfo.BaritoneHornBb, InstrumentInfo.TenorHornBb, InstrumentInfo.TromboneBb] },
        { InstrumentInfo.BassBb, [InstrumentInfo.BassClarinetBb] },
        { InstrumentInfo.Bassoon, [InstrumentInfo.BassC, InstrumentInfo.BaritoneHornC, InstrumentInfo.BassTromboneC, InstrumentInfo.TromboneC] },
        { InstrumentInfo.AltoSaxophoneEb, [InstrumentInfo.TenorHornEb] },
        { InstrumentInfo.TenorSaxophoneBb, [InstrumentInfo.TenorHornBb] },
        { InstrumentInfo.BaritoneSaxophoneEb, [InstrumentInfo.BassEb, InstrumentInfo.HornEb] },
        { InstrumentInfo.HornEb, [InstrumentInfo.TenorHornEb] },
        { InstrumentInfo.BassEb, [InstrumentInfo.BaritoneSaxophoneEb, InstrumentInfo.HornEb] },
        { InstrumentInfo.TromboneC, [InstrumentInfo.BassTromboneC, InstrumentInfo.Bassoon, InstrumentInfo.BassC] }
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
