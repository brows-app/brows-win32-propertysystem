using Brows.Win32.InteropServices;
using Brows.Win32.InteropServices.ComTypes;
using Brows.Win32.PlatformInvoke;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Brows.Win32;

/// <summary>
/// Reads and writes file metadata through the Windows Property System.
/// </summary>
public static class PropertySystem {
    /// <summary>
    /// Enumerates every viewable property registered in the system property schema.
    /// </summary>
    /// <returns>
    /// The registered property descriptions. Properties without a canonical name are skipped.
    /// </returns>
    /// <exception cref="COMException">The property schema could not be enumerated.</exception>
    public static IEnumerable<PropertyDescription> EnumeratePropertyDescriptions() {
        var pdl = default(IPropertyDescriptionList);
        var pdlIID = IID.Managed.IPropertyDescriptionList;
        var
        hr = propsys.PSEnumeratePropertyDescriptions(PROPDESC_ENUMFILTER.VIEWABLE, ref pdlIID, out pdl);
        hr.ThrowOnError();
        try {
            hr = pdl.GetCount(out var pcElem);
            hr.ThrowOnError();
            for (uint i = 0; i < pcElem; i++) {
                var pd = default(IPropertyDescription);
                var pdIID = IID.Managed.IPropertyDescription;
                hr = pdl.GetAt(i, ref pdIID, out pd);
                hr.ThrowOnError();
                try {
                    var item = PropertyDescription.Of(pd);
                    if (item != null) {
                        yield return item;
                    }
                }
                finally {
                    Marshal.ReleaseComObject(pd);
                }
            }
        }
        finally {
            Marshal.ReleaseComObject(pdl);
        }
    }

    /// <summary>
    /// Enumerates the properties present in a file's property store.
    /// </summary>
    /// <param name="fileName">The path of the file to read.</param>
    /// <param name="throwOnError">
    /// <see langword="true"/> to propagate failures to open the file's property store;
    /// <see langword="false"/> to yield no properties instead.
    /// </param>
    /// <returns>
    /// The properties the file carries, which is a small subset of the system property schema and
    /// depends on the property handler registered for the file's type. Properties that are not
    /// registered in the schema are skipped.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// <paramref name="fileName"/> does not exist and <paramref name="throwOnError"/> is
    /// <see langword="true"/>. Thrown when enumeration starts, not when this method is called.
    /// </exception>
    /// <exception cref="COMException">
    /// The file's property store could not be opened or read and <paramref name="throwOnError"/>
    /// is <see langword="true"/>. Thrown when enumeration starts, not when this method is called.
    /// </exception>
    public static IEnumerable<PropertyDescription> EnumeratePropertyDescriptions(string fileName, bool throwOnError) {
        using var wrap = new PropertyStoreWrapper(fileName, readOnly: true);
        var propertyStore = default(IPropertyStore);
        try {
            propertyStore = wrap.PropertyStore;
        }
        catch {
            if (throwOnError) {
                throw;
            }
            yield break;
        }
        var iid = IID.Managed.IPropertyDescription;
        var
        hr = propertyStore.GetCount(out var cProps);
        hr.ThrowOnError();
        for (uint i = 0; i < cProps; i++) {
            hr = propertyStore.GetAt(i, out var pkey);
            hr.ThrowOnError();
            if (pkey.pid < 2) {
                continue;
            }
            var e = hr = propsys.PSGetPropertyDescription(ref pkey, ref iid, out var ppv);
            if (e == HRESULT.TYPE_E_ELEMENTNOTFOUND) {
                continue;
            }
            else {
                hr.ThrowOnError();
            }
            try {
                var item = PropertyDescription.Of(ppv);
                if (item != null) {
                    yield return item;
                }
            }
            finally {
                Marshal.ReleaseComObject(ppv);
            }
        }
    }

    /// <summary>
    /// Reads a single property value from a file.
    /// </summary>
    /// <param name="fileName">The path of the file to read.</param>
    /// <param name="propertyDescription">The property to read.</param>
    /// <param name="throwOnError">
    /// <see langword="true"/> to propagate failures to open the file's property store;
    /// <see langword="false"/> to return <see langword="null"/> instead.
    /// </param>
    /// <returns>
    /// The property value, or <see langword="null"/> if the property store could not be opened and
    /// <paramref name="throwOnError"/> is <see langword="false"/>. A property the file has never
    /// had set still returns a value, with an empty <see cref="PropertyValue.Display"/> and a
    /// <see langword="null"/> <see cref="PropertyValue.Object"/>.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// <paramref name="fileName"/> does not exist and <paramref name="throwOnError"/> is
    /// <see langword="true"/>.
    /// </exception>
    /// <exception cref="COMException">
    /// The value could not be read and <paramref name="throwOnError"/> is <see langword="true"/>.
    /// </exception>
    public static PropertyValue GetPropertyValue(string fileName,
                                                 PropertyDescription propertyDescription,
                                                 bool throwOnError) {
        return EnumeratePropertyValues(fileName, new[] { propertyDescription }, throwOnError).FirstOrDefault();
    }

    /// <summary>
    /// Reads several property values from a file, opening its property store only once.
    /// </summary>
    /// <param name="fileName">The path of the file to read.</param>
    /// <param name="propertyDescriptions">The properties to read.</param>
    /// <param name="throwOnError">
    /// <see langword="true"/> to propagate failures to open the file's property store;
    /// <see langword="false"/> to yield no values instead.
    /// </param>
    /// <returns>
    /// One value per requested property, in the order requested.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyDescriptions"/> is <see langword="null"/>. Thrown when enumeration
    /// starts, not when this method is called.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// <paramref name="fileName"/> does not exist and <paramref name="throwOnError"/> is
    /// <see langword="true"/>. Thrown when enumeration starts, not when this method is called.
    /// </exception>
    /// <exception cref="COMException">
    /// A value could not be read and <paramref name="throwOnError"/> is <see langword="true"/>.
    /// Thrown when enumeration starts, not when this method is called.
    /// </exception>
    public static IEnumerable<PropertyValue>
    EnumeratePropertyValues(string fileName,
                            IEnumerable<PropertyDescription> propertyDescriptions,
                            bool throwOnError) {
        if (null == propertyDescriptions) throw new ArgumentNullException(nameof(propertyDescriptions));
        using var wrap = new PropertyStoreWrapper(fileName, readOnly: true);
        var propertyStore = default(IPropertyStore);
        try {
            propertyStore = wrap.PropertyStore;
        }
        catch {
            if (throwOnError) {
                throw;
            }
            yield break;
        }
        foreach (var propertyDescription in propertyDescriptions) {
            var key = propertyDescription.Key;
            var pv = default(PROPVARIANT);
            var
            hr = propertyStore.GetValue(ref key, ref pv);
            hr.ThrowOnError();
            try {
                var variantObj = default(object);
                var variant = Marshal.AllocCoTaskMem(Environment.Is64BitProcess ? 24 : 16);
                try {
                    oleaut32.VariantInit(variant);
                    try {
                        hr = propsys.PropVariantToVariant(ref pv, variant);
                        hr.ThrowOnError();
                        try {
                            variantObj = Marshal.GetObjectForNativeVariant(variant);
                        }
                        catch (InvalidOleVariantTypeException) {
                        }
                    }
                    finally {
                        oleaut32.VariantClear(variant);
                    }
                }
                finally {
                    Marshal.FreeCoTaskMem(variant);
                }
                hr = propsys.PSFormatForDisplayAlloc(ref key,
                                                     ref pv,
                                                     PROPDESC_FORMAT_FLAGS.DEFAULT,
                                                     out var ppszDisplay);
                hr.ThrowOnError();
                try {
                    yield return new PropertyValue(description: propertyDescription,
                                                   display: Marshal.PtrToStringUni(ppszDisplay),
                                                   @object: variantObj);
                }
                finally {
                    ole32.CoTaskMemFree(ppszDisplay);
                }
            }
            finally {
                ole32.PropVariantClear(ref pv);
            }
        }
    }

    /// <summary>
    /// Looks up a property in the system property schema by name.
    /// </summary>
    /// <param name="name">
    /// A canonical property name such as <c>System.Photo.DateTaken</c>, or one of the legacy names
    /// Windows still accepts such as <c>WhenTaken</c>. Canonical names are case-sensitive.
    /// </param>
    /// <returns>The property description.</returns>
    /// <exception cref="COMException">
    /// <paramref name="name"/> is not registered in the property schema, in which case the
    /// <see cref="Exception.HResult"/> is <c>TYPE_E_ELEMENTNOTFOUND</c> (<c>0x8002802B</c>).
    /// </exception>
    public static PropertyDescription GetPropertyDescription(string name) {
        var iid = IID.Managed.IPropertyDescription;
        var hr = propsys.PSGetPropertyDescriptionByName(name, ref iid, out var ppv);
        hr.ThrowOnError();
        try {
            return PropertyDescription.Of(ppv);
        }
        finally {
            Marshal.ReleaseComObject(ppv);
        }
    }

    /// <summary>
    /// Writes a single property value to a file and commits the change.
    /// </summary>
    /// <param name="fileName">The path of the file to write.</param>
    /// <param name="propertyDescription">The property to write.</param>
    /// <param name="propertyValue">
    /// The value to write, coerced to the property's canonical type before being stored.
    /// </param>
    /// <exception cref="COMException">
    /// The file's property store could not be opened for writing, or the property could not be
    /// written. A file type with no writable property handler fails with <c>E_FAIL</c>
    /// (<c>0x80004005</c>), and a read-only or calculated property such as <c>System.Size</c>
    /// fails with <c>WINCODEC_ERR_PROPERTYNOTSUPPORTED</c> (<c>0x88982F41</c>).
    /// </exception>
    public static void SetPropertyValue(string fileName,
                                        PropertyDescription propertyDescription,
                                        object propertyValue) {
        SetPropertyValues(
            fileName,
            new[] { new KeyValuePair<PropertyDescription, object>(propertyDescription, propertyValue) },
            ignoreError: null);
    }

    /// <summary>
    /// Writes several property values to a file and commits them as a single change.
    /// </summary>
    /// <param name="fileName">The path of the file to write.</param>
    /// <param name="propertyValues">
    /// The properties to write, paired with the values to write to them. Each value is coerced to
    /// its property's canonical type before being stored.
    /// </param>
    /// <param name="ignoreError">
    /// An optional callback invoked when writing an individual property fails, receiving the
    /// property being written and the failing <c>HRESULT</c> as an unsigned 32-bit value. Return
    /// <see langword="true"/> to skip that property and carry on, or <see langword="false"/> to
    /// let the failure propagate. Pass <see langword="null"/> to let every failure propagate.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyValues"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="COMException">
    /// The file's property store could not be opened for writing, a property could not be written
    /// and was not ignored, or the commit failed. A file type with no writable property handler
    /// fails with <c>E_FAIL</c> (<c>0x80004005</c>), and a read-only or calculated property such as
    /// <c>System.Size</c> fails with <c>WINCODEC_ERR_PROPERTYNOTSUPPORTED</c> (<c>0x88982F41</c>).
    /// </exception>
    public static void SetPropertyValues(string fileName,
                                         IEnumerable<KeyValuePair<PropertyDescription, object>> propertyValues,
                                         Func<KeyValuePair<PropertyDescription, object>, long, bool> ignoreError) {
        if (null == propertyValues) throw new ArgumentNullException(nameof(propertyValues));
        using var wrap = new PropertyStoreWrapper(fileName, readOnly: false);
        var propertyStore = wrap.PropertyStore;
        var hr = default(HRESULT);
        foreach (var propertyValue in propertyValues) {
            var propertyDescription = propertyValue.Key;
            var key = propertyDescription.Key;
            var obj = propertyValue.Value;
            var variant = Marshal.AllocCoTaskMem(Environment.Is64BitProcess ? 24 : 16);
            try {
                Marshal.GetNativeVariantForObject(obj, variant);
                try {
                    hr = propsys.VariantToPropVariant(variant, out var propVariant);
                    hr.ThrowOnError();
                    try {
                        hr = propsys.PSCoerceToCanonicalValue(ref key, ref propVariant);
                        hr.ThrowOnError();
                        try {
                            hr = propertyStore.SetValue(ref key, ref propVariant);
                            hr.ThrowOnError();
                        }
                        catch (Exception ex) {
                            var error = unchecked((uint)ex.HResult);
                            var ignore = ignoreError != null && ignoreError(propertyValue, error);
                            if (ignore == false) {
                                throw;
                            }
                        }
                    }
                    finally {
                        ole32.PropVariantClear(ref propVariant);
                    }
                }
                finally {
                    oleaut32.VariantClear(variant);
                }
            }
            finally {
                Marshal.FreeCoTaskMem(variant);
            }
        }
        hr = propertyStore.Commit();
        hr.ThrowOnError();
    }

    /// <summary>
    /// Copies every property value from one file to another and commits them as a single change.
    /// </summary>
    /// <param name="sourceFileName">The path of the file to read properties from.</param>
    /// <param name="destinationFileName">The path of the file to write properties to.</param>
    /// <param name="ignoreProperty">
    /// An optional callback invoked for each property read from the source, receiving the property
    /// and its value. Return <see langword="true"/> to leave that property out of the transfer.
    /// Pass <see langword="null"/> to transfer every property.
    /// </param>
    /// <param name="ignoreError">
    /// An optional callback invoked when writing an individual property to the destination fails,
    /// receiving the property being written and the failing <c>HRESULT</c> as an unsigned 32-bit
    /// value. Return <see langword="true"/> to skip that property and carry on, or
    /// <see langword="false"/> to let the failure propagate. Pass <see langword="null"/> to let
    /// every failure propagate. Because the source file's read-only and calculated properties are
    /// transferred too, a callback is normally needed here.
    /// </param>
    /// <exception cref="FileNotFoundException">
    /// <paramref name="sourceFileName"/> does not exist.
    /// </exception>
    /// <exception cref="COMException">
    /// The source could not be read, the destination could not be opened for writing, a property
    /// could not be written and was not ignored, or the commit failed.
    /// </exception>
    public static void
    TransferPropertyValues(string sourceFileName,
                           string destinationFileName,
                           Func<KeyValuePair<PropertyDescription, object>, bool> ignoreProperty,
                           Func<KeyValuePair<PropertyDescription, object>, long, bool> ignoreError) {
        var set = new Dictionary<PropertyDescription, object>();
        var sourceProperties = EnumeratePropertyDescriptions(sourceFileName, throwOnError: true);
        var propertyDescriptions = sourceProperties.ToList();
        var propertyValues = EnumeratePropertyValues(sourceFileName, propertyDescriptions, throwOnError: true);
        foreach (var propertyValue in propertyValues) {
            var key = propertyValue.Description;
            var val = propertyValue.Object;
            if (ignoreProperty == null || ignoreProperty(new(key, val)) == false) {
                set[key] = val;
            }
        }
        SetPropertyValues(destinationFileName, set, ignoreError);
    }
}
