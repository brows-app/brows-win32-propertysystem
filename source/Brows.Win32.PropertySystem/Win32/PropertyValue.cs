namespace Brows.Win32;

/// <summary>
/// A single property value read from a file's property store.
/// </summary>
public sealed class PropertyValue {
    /// <summary>
    /// The property this value belongs to.
    /// </summary>
    public PropertyDescription Description { get; }

    /// <summary>
    /// The value formatted for display, as produced by the property's own formatting rules.
    /// This is an empty string when the property has no value.
    /// </summary>
    public string Display { get; }

    /// <summary>
    /// The value as a managed object, or <see langword="null"/> when the property has no value or
    /// its type cannot be represented as a managed object. The runtime type depends on the
    /// property; for example <c>System.Size</c> yields a <see cref="ulong"/> and
    /// <c>System.Keywords</c> yields a <see cref="string"/> array.
    /// </summary>
    public object Object { get; }

    /// <summary>
    /// Creates a property value.
    /// </summary>
    /// <param name="description">The property this value belongs to.</param>
    /// <param name="display">The value formatted for display.</param>
    /// <param name="object">The value as a managed object.</param>
    public PropertyValue(PropertyDescription description, string display, object @object) {
        Description = description;
        Display = display;
        Object = @object;
    }
}
