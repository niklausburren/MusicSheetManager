using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MusicSheetManager.Models;

public class VoiceInfo
{
    public static VoiceInfo Unknown { get; } = new VoiceInfo("Unknown");

    public static VoiceInfo First { get; } = new VoiceInfo("First", "1st", "Ist", "lst", "1");

    public static VoiceInfo Second { get; } = new VoiceInfo("Second", "2nd", "2");

    public static VoiceInfo Third { get; } = new VoiceInfo("Third", "3rd", "3");

    public static VoiceInfo Fourth { get; } = new VoiceInfo("Fourth", "4th", "4");

    public static VoiceInfo Part1 { get; } = new VoiceInfo("Part1", "Part 1");

    public static VoiceInfo Part2 { get; } = new VoiceInfo("Part2", "Part 2");

    public static VoiceInfo Part3 { get; } = new VoiceInfo("Part3", "Part 3");

    public static VoiceInfo Part4 { get; } = new VoiceInfo("Part4", "Part 4");

    public static VoiceInfo Part5 { get; } = new VoiceInfo("Part5", "Part 5");


    public static IEnumerable<VoiceInfo> All => typeof(VoiceInfo)
        .GetProperties(BindingFlags.Static | BindingFlags.Public)
        .Select(pi => pi.GetValue(null))
        .OfType<VoiceInfo>();

    public static IEnumerable<VoiceInfo> TryGet(string text, InstrumentInfo instrument)
    {
        foreach (var instrumentName in instrument.Names)
        {
            if (text.Contains(instrumentName, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var voice in All)
                {
                    foreach (var voiceName in voice.Names)
                    {
                        if (ContainsVoice(text, instrumentName, voiceName, 12))
                        {
                            yield return voice;
                            break;
                        }
                    }
                }

                break;
            }
        }
    }

    private static bool ContainsVoice(string text, string instrumentName, string voiceName, int contextLength)
    {
        // Suche nach dem ersten Substring (instrumentKey)
        int index = text.IndexOf(instrumentName, StringComparison.OrdinalIgnoreCase);

        if (index != -1)
        {
            // Berechnung der Start- und Endpositionen für den Bereich um instrumentKey
            int start = Math.Max(0, index - contextLength);
            int length = Math.Min(text.Length - start, instrumentName.Length + 2 * contextLength);

            string surroundingText = text.Substring(start, length);

            // Prüfung auf voiceKey im umgebenden Bereich
            return surroundingText.Contains(voiceName, StringComparison.OrdinalIgnoreCase);
        }

        // Falls instrumentKey nicht gefunden wurde, gibt die Funktion false zurück
        return false;
    }

    private VoiceInfo(string key, params string[] names)
    {
        Key = key;
        Names = names;
    }

    public string Key { get; }
    public string[] Names { get; }
}
