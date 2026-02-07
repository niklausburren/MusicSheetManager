using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

// ReSharper disable StringLiteralTypo

namespace MusicSheetManager.Models
{
    public class ClefInfo
    {
        #region Constructors

        private ClefInfo(string key, params string[] patterns)
        {
            this.Key = key;
            this.Patterns = patterns;
        }

        #endregion


        #region Properties

        public static ClefInfo TrebleClef { get; } = new("TrebleClef", "TC", "T.C.");

        public static ClefInfo BassClef { get; } = new("BassClef", "BC", "B.C.");

        public static ClefInfo PercussionClef { get; } = new("PercussionClef", "Perc", "Perc.");

        public static IReadOnlyList<ClefInfo> All { get; } = typeof(ClefInfo)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Select(pi => pi.GetValue(null))
            .OfType<ClefInfo>()
            .ToList();

        public string Key { get; }

        public string[] Patterns { get; }

        public string DisplayName => this.Patterns.First();

        #endregion


        #region Public Methods

        /// <inheritdoc />
        public override string ToString()
        {
            return this.DisplayName;
        }

        public static ClefInfo GetByKey(string key)
        {
            return All.Single(clef => clef.Key == key);
        }

        public static ClefInfo TryGet(string text, InstrumentInfo instrument)
        {
            if (instrument == InstrumentInfo.StringBass ||
                instrument == InstrumentInfo.Timpani ||
                instrument == InstrumentInfo.BassDrum ||
                instrument == InstrumentInfo.Bassoon ||
                instrument == InstrumentInfo.TromboneC ||
                instrument == InstrumentInfo.BassTromboneC ||
                instrument == InstrumentInfo.EuphoniumC ||
                instrument == InstrumentInfo.TenorHornC ||
                instrument == InstrumentInfo.BaritoneHornC ||
                instrument == InstrumentInfo.BassC)
            {
                return BassClef;
            }

            if (instrument == InstrumentInfo.Percussion ||
                instrument == InstrumentInfo.DrumSet ||
                instrument == InstrumentInfo.SnareDrum ||
                instrument == InstrumentInfo.Cymbals ||
                instrument == InstrumentInfo.Triangle)
            {
                return PercussionClef;
            }

            foreach (var instrumentPattern in instrument.Patterns)
            {
                if (!Regex.Match(text, instrumentPattern, RegexOptions.IgnoreCase).Success)
                {
                    continue;
                }

                foreach (var clef in All)
                {
                    if (clef.Patterns.Any(clefPattern => ContainsClef(text, instrumentPattern, clefPattern, 10)))
                    {
                        return clef;
                    }
                }

                break;
            }

            return TrebleClef;
        }

        #endregion


        #region Private Methods

        private static bool ContainsClef(string text, string instrumentPattern, string clefPattern, int contextLength)
        {
            var result = Regex.Match(text, instrumentPattern, RegexOptions.IgnoreCase);

            if (!result.Success)
            {
                return false;
            }

            var start = result.Index;
            var length = Math.Min(text.Length - start, contextLength);

            return text.Substring(start, length).Contains(clefPattern, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}
