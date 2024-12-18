using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MusicSheetManager.Models;

public class InstrumentInfo
{
    public static InstrumentInfo Unknown { get; } = new InstrumentInfo("Unknown");

    public static InstrumentInfo Piccolo { get; } = new InstrumentInfo("Piccolo", "Piccolo");

    public static InstrumentInfo Flute { get; } = new InstrumentInfo("Flute", "Flute");

    public static InstrumentInfo Oboe { get; } = new InstrumentInfo("Oboe", "Oboe");

    public static InstrumentInfo Bassoon { get; } = new InstrumentInfo("Bassoon", "Bassoon");

    public static InstrumentInfo EbClarinet { get; } = new InstrumentInfo("EbClarinet", "Eb Clarinet");

    public static InstrumentInfo BbClarinet { get; } = new InstrumentInfo("BbClarinet", "Bb Clarinet");

    public static InstrumentInfo BbAltoClarinet { get; } = new InstrumentInfo("BbAltoClarinet", "Eb Alto Clarinet");

    public static InstrumentInfo BbBassClarinet { get; } = new InstrumentInfo("BbBassClarinet", "Bb Bass Clarinet", "Bass Clarinet");

    public static InstrumentInfo EbAltoSaxophone { get; } = new InstrumentInfo("EbAltoSaxophone", "Eb Alto Saxophone");

    public static InstrumentInfo BbTenorSaxophone { get; } = new InstrumentInfo("BbTenorSaxophone", "Bb Tenor Saxophone");

    public static InstrumentInfo EbBaritoneSaxophone { get; } = new InstrumentInfo("EbBaritoneSaxophone", "Eb Baritone Saxophone");

    public static InstrumentInfo BbTrumpet { get; } = new InstrumentInfo("BbTrumpet", "Bb Trumpet", "Trumpet");

    public static InstrumentInfo FHorn { get; } = new InstrumentInfo("FHorn", "F Horn");

    public static InstrumentInfo EbHorn { get; } = new InstrumentInfo("EbHorn", "Eb Horn");

    public static InstrumentInfo BbTrombones { get; } = new InstrumentInfo("BbTrombone", "Bb Trombone");

    public static InstrumentInfo BbEuphonium { get; } = new InstrumentInfo("BbEuphonium", "Bb Euphonium");

    public static InstrumentInfo EbBass { get; } = new InstrumentInfo("EbBass", "Eb Bass");

    public static InstrumentInfo BbBass { get; } = new InstrumentInfo("BbBass", "Bb Bass");

    public static InstrumentInfo DrumSet { get; } = new InstrumentInfo("DrumSet", "Drum Set");

    public static InstrumentInfo Timpani { get; } = new InstrumentInfo("Timpani", "Timpani");

    public static InstrumentInfo Mallets { get; } = new InstrumentInfo("Mallets", "Mallets");

    public static InstrumentInfo Percussion { get; } = new InstrumentInfo("Percussion", "Percussion");

    public static IEnumerable<InstrumentInfo> All => typeof(InstrumentInfo)
        .GetProperties(BindingFlags.Static | BindingFlags.Public)
        .Select(pi => pi.GetValue(null))
        .OfType<InstrumentInfo>();

    public static InstrumentInfo TryGet(string text)
    {
        var closestInstrument = Unknown;
        int closestPosition = int.MaxValue;

        foreach (var instrumentInfo in All)
        {
            foreach (var name in instrumentInfo.Names)
            {
                int position = text.IndexOf(name, StringComparison.OrdinalIgnoreCase);
                if (position >= 0 && position < closestPosition)
                {
                    closestInstrument = instrumentInfo;
                    closestPosition = position;
                }
            }
        }

        return closestInstrument;
    }

    private InstrumentInfo(string key, params string[] names)
    {
        Key = key;
        Names = names;
    }

    public string Key { get; }
    public string[] Names { get; }
}
