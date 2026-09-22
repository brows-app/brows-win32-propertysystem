using Brows.Win32.InteropServices;
using Brows.Win32.InteropServices.ComTypes;
using Brows.Win32.PlatformInvoke;

namespace Brows.Win32;

/// <summary>
/// Describes a property in the Windows Property System, such as <c>System.Photo.DateTaken</c>.
/// </summary>
public sealed class PropertyDescription {
    private PropertyDescription(string canonicalName, string displayName, PROPERTYKEY key) {
        CanonicalName = canonicalName;
        DisplayName = displayName;
        Key = key;
    }

    internal PROPERTYKEY Key { get; }

    internal static PropertyDescription Of(IPropertyDescription item) {
        var key = item.GetPropertyKey();
        if (key.fmtid == Guid.Empty) {
            return null;
        }
        var canonicalName = item.GetCanonicalName();
        if (canonicalName == null) {
            return null;
        }
        var displayName = item.GetDisplayName();
        return new PropertyDescription(
            canonicalName: canonicalName,
            displayName: displayName,
            key: key);
    }

    /// <summary>
    /// The property's canonical name, for example <c>System.Photo.DateTaken</c>.
    /// Canonical names are case-sensitive.
    /// </summary>
    public string CanonicalName { get; }

    /// <summary>
    /// The property's localized display name, for example <c>Date taken</c>.
    /// This is an empty string for properties that are not meant to be shown to a user.
    /// </summary>
    public string DisplayName { get; }
}
