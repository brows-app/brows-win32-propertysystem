using System.Collections.Generic;

namespace Brows.Win32;

/// <summary>
/// Matches canonical property names against a dot-separated pattern, where a <c>*</c> segment
/// matches that segment and every segment after it. For example <c>System.Photo.*</c> matches
/// <c>System.Photo.DateTaken</c>, while <c>System.Photo</c> matches only <c>System.Photo</c> itself.
/// </summary>
public sealed class PropertyWildcard {
    /// <summary>
    /// The dot-separated segments of <see cref="Name"/>.
    /// </summary>
    public IReadOnlyList<string> Parts => field ??= Name.Split('.');

    /// <summary>
    /// The pattern this wildcard was created from.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Creates a wildcard from a dot-separated pattern.
    /// </summary>
    /// <param name="name">
    /// The pattern, for example <c>System.Photo.*</c>. A <c>*</c> segment matches that segment and
    /// every segment after it.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    public PropertyWildcard(string name) {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Determines whether a property name matches this wildcard.
    /// </summary>
    /// <param name="name">
    /// The property name to test, for example <c>System.Photo.DateTaken</c>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="name"/> matches; otherwise <see langword="false"/>.
    /// Segments are compared case-insensitively. Unless the pattern contains a <c>*</c> segment,
    /// <paramref name="name"/> must have exactly as many segments as the pattern, so a pattern
    /// without a <c>*</c> is not treated as a prefix. A <see langword="null"/>
    /// <paramref name="name"/> never matches.
    /// </returns>
    public bool Matches(string name) {
        if (name == null) {
            return false;
        }
        var other = name.Split('.');
        for (var i = 0; i < Parts.Count; i++) {
            var thisPart = Parts[i];
            if (thisPart == "*") {
                return true;
            }
            if (other.Length <= i) {
                return false;
            }
            var otherPart = other[i];
            if (otherPart.Equals(thisPart, StringComparison.OrdinalIgnoreCase) == false) {
                return false;
            }
        }
        return other.Length == Parts.Count;
    }
}
