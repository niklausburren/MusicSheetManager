using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace MusicSheetManager.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public class PartInfo : IEquatable<PartInfo>
{
    #region Constructors

    private PartInfo(string key, params string[] patterns)
    {
        this.Key = key;
        this.Patterns = patterns;
    }

    #endregion


    #region Properties

    public static PartInfo None { get; } = new("None", "(None)");

    public static PartInfo First { get; } = new("First", "1st", "Ist", "lst", "1");

    public static PartInfo Second { get; } = new("Second", "2nd", "2");

    public static PartInfo Third { get; } = new("Third", "3rd", "3");

    public static PartInfo Fourth { get; } = new("Fourth", "4th", "4");

    public static PartInfo Fifth { get; } = new("Fifth", "5th", "5");

    public static IReadOnlyList<PartInfo> All { get; } = typeof(PartInfo)
        .GetProperties(BindingFlags.Static | BindingFlags.Public)
        .Select(pi => pi.GetValue(null))
        .OfType<PartInfo>()
        .ToList();

    public static IReadOnlyList<PartInfo> AllWithoutNone { get; } = All.Where(p => p != None).ToList();

    public string Key { get; }

    public int Index => All.ToList().IndexOf(this);

    public string[] Patterns { get; }

    public string DisplayName => this.Patterns.First();

    #endregion


    #region Public Methods

    public IEnumerable<PartInfo> GetSelfAndHigherParts()
    {
        return new[] { this }.Concat(All.Where(p => p.Index < this.Index).OrderByDescending(p => p.Index));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.DisplayName;
    }

    public static PartInfo GetByKey(string key)
    {
        return All.Single(clef => clef.Key == key);
    }

    public static IEnumerable<PartInfo> TryGet(string text, InstrumentInfo instrument)
    {
        foreach (var instrumentPattern in instrument.Patterns)
        {
            if (!Regex.Match(text, instrumentPattern, RegexOptions.IgnoreCase).Success)
            {
                continue;
            }

            foreach (var part in All)
            {
                if (part.Patterns.Any(partPattern => ContainsPart(text, instrumentPattern, partPattern, 10)))
                {
                    yield return part;
                }
            }

            break;
        }
    }

    /// <summary>
    /// The equality operator.
    /// </summary>
    public static bool operator ==(PartInfo left, PartInfo right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// The inequality operator.
    /// </summary>
    public static bool operator !=(PartInfo left, PartInfo right)
    {
        return !Equals(left, right);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is null)
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

        return this.Equals((PartInfo)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return this.Key != null
            ? this.Key.GetHashCode()
            : 0;
    }

    #endregion


    #region Private Methods

    private static bool ContainsPart(string text, string instrumentPattern, string partPattern, int contextLength)
    {
        var result = Regex.Match(text, instrumentPattern, RegexOptions.IgnoreCase);

        if (!result.Success)
        {
            return false;
        }

        var start = Math.Max(0, result.Index - contextLength);
        var length = Math.Min(text.Length - start, instrumentPattern.Length + 2 * contextLength);

        var surroundingText = text.Substring(start, length);
        return surroundingText.Contains(partPattern, StringComparison.OrdinalIgnoreCase);

    }

    #endregion


    #region IEquatable<PartInfo> Members

    /// <inheritdoc />
    public bool Equals(PartInfo other)
    {
        if (other is null)
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
