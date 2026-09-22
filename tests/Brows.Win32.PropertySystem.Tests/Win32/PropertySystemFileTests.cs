using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Brows.Win32.Tests;

[TestFixture]
public class PropertySystemFileTests {
    private static PropertyDescription Title
        => PropertySystem.GetPropertyDescription("System.Title");

    private static PropertyDescription Keywords
        => PropertySystem.GetPropertyDescription("System.Keywords");

    private static PropertyDescription Size
        => PropertySystem.GetPropertyDescription("System.Size");

    [Test]
    public void EnumeratePropertyDescriptions_ExistingFile_ReturnsFileProperties() {
        var file = TemporaryTestDirectory.CreateJpegFile();

        var descriptions = PropertySystem.EnumeratePropertyDescriptions(file, throwOnError: true).ToList();

        Assert.That(descriptions, Is.Not.Empty);
        Assert.That(descriptions.Select(d => d.CanonicalName), Does.Contain("System.Size"));
        Assert.That(descriptions.Select(d => d.CanonicalName), Does.Contain("System.ItemNameDisplay"));
    }

    [Test]
    public void EnumeratePropertyDescriptions_NonexistentFile_ThrowOnErrorTrue_Throws() {
        var file = Path.Combine(TemporaryTestDirectory.Root, Guid.NewGuid().ToString("N") + ".jpg");

        var ex = Assert.Throws<FileNotFoundException>(
            () => PropertySystem.EnumeratePropertyDescriptions(file, throwOnError: true).ToList());

        Assert.That(ex.HResult, Is.EqualTo(unchecked((int)0x80070002)));
    }

    [Test]
    public void EnumeratePropertyDescriptions_NonexistentFile_ThrowOnErrorFalse_ReturnsEmpty() {
        var file = Path.Combine(TemporaryTestDirectory.Root, Guid.NewGuid().ToString("N") + ".jpg");

        var descriptions = PropertySystem.EnumeratePropertyDescriptions(file, throwOnError: false).ToList();

        Assert.That(descriptions, Is.Empty);
    }

    [Test]
    public void GetPropertyValue_Size_MatchesActualFileLength() {
        var file = TemporaryTestDirectory.CreateJpegFile();

        var value = PropertySystem.GetPropertyValue(file, Size, throwOnError: true);

        Assert.That(value.Object, Is.EqualTo((ulong)new FileInfo(file).Length));
    }

    [Test]
    public void GetPropertyValue_NeverSetProperty_ReturnsValueWithNullObject() {
        var file = TemporaryTestDirectory.CreateJpegFile();

        var value = PropertySystem.GetPropertyValue(file, Keywords, throwOnError: true);

        Assert.Multiple(() => {
            Assert.That(value, Is.Not.Null);
            Assert.That(value.Object, Is.Null);
        });
    }

    [Test]
    public void SetPropertyValue_ThenGetPropertyValue_RoundTripsTitle() {
        var file = TemporaryTestDirectory.CreateJpegFile();

        PropertySystem.SetPropertyValue(file, Title, "My Test Title");
        var value = PropertySystem.GetPropertyValue(file, Title, throwOnError: true);

        Assert.Multiple(() => {
            Assert.That(value.Object, Is.EqualTo("My Test Title"));
            Assert.That(value.Display, Is.EqualTo("My Test Title"));
        });
    }

    [Test]
    public void SetPropertyValues_MultipleProperties_RoundTripsAll() {
        var file = TemporaryTestDirectory.CreateJpegFile();

        PropertySystem.SetPropertyValues(file, new Dictionary<PropertyDescription, object> {
            { Title, "Multi Title" },
            { Keywords, new[] { "alpha", "beta" } },
        }, ignoreError: null);

        var title = PropertySystem.GetPropertyValue(file, Title, throwOnError: true);
        var keywords = PropertySystem.GetPropertyValue(file, Keywords, throwOnError: true);

        Assert.Multiple(() => {
            Assert.That(title.Object, Is.EqualTo("Multi Title"));
            Assert.That(keywords.Object, Is.EqualTo(new[] { "alpha", "beta" }));
        });
    }

    [Test]
    public void SetPropertyValue_ReadOnlyCalculatedProperty_Throws() {
        var file = TemporaryTestDirectory.CreateJpegFile();

        var ex = Assert.Throws<System.Runtime.InteropServices.COMException>(
            () => PropertySystem.SetPropertyValue(file, Size, (ulong)123));

        // WINCODEC_ERR_PROPERTYNOTSUPPORTED
        Assert.That(ex.HResult, Is.EqualTo(unchecked((int)0x88982F41)));
    }

    [Test]
    public void SetPropertyValues_IgnoreErrorReturnsTrue_SwallowsFailure() {
        var file = TemporaryTestDirectory.CreateJpegFile();

        Assert.DoesNotThrow(() => PropertySystem.SetPropertyValues(file, new Dictionary<PropertyDescription, object> {
            { Size, (ulong)123 },
        }, ignoreError: (kv, error) => true));
    }

    [Test]
    public void SetPropertyValues_IgnoreErrorReturnsFalse_Throws() {
        var file = TemporaryTestDirectory.CreateJpegFile();

        Assert.Throws<System.Runtime.InteropServices.COMException>(
            () => PropertySystem.SetPropertyValues(file, new Dictionary<PropertyDescription, object> {
                { Size, (ulong)123 },
            }, ignoreError: (kv, error) => false));
    }

    [Test]
    public void SetPropertyValues_IgnoreErrorReceivesFailingPropertyAndErrorCode() {
        var file = TemporaryTestDirectory.CreateJpegFile();
        var size = Size;
        KeyValuePair<PropertyDescription, object> observedProperty = default;
        long observedError = 0;

        PropertySystem.SetPropertyValues(file, new Dictionary<PropertyDescription, object> {
            { size, (ulong)123 },
        }, ignoreError: (kv, error) => {
            observedProperty = kv;
            observedError = error;
            return true;
        });

        Assert.Multiple(() => {
            Assert.That(observedProperty.Key, Is.SameAs(size));
            Assert.That(observedError, Is.EqualTo(0x88982F41));
        });
    }

    [Test]
    public void SetPropertyValues_NullPropertyValues_ThrowsArgumentNullException() {
        var file = TemporaryTestDirectory.CreateJpegFile();

        var ex = Assert.Throws<ArgumentNullException>(
            () => PropertySystem.SetPropertyValues(file, null, ignoreError: null));

        Assert.That(ex.ParamName, Is.EqualTo("propertyValues"));
    }

    [Test]
    public void EnumeratePropertyValues_NullPropertyDescriptions_ThrowsOnEnumeration() {
        var file = TemporaryTestDirectory.CreateJpegFile();

        // EnumeratePropertyValues is an iterator method: argument validation is deferred
        // until the sequence is actually enumerated.
        var sequence = PropertySystem.EnumeratePropertyValues(file, null, throwOnError: true);

        var ex = Assert.Throws<ArgumentNullException>(() => sequence.ToList());
        Assert.That(ex.ParamName, Is.EqualTo("propertyDescriptions"));
    }

    [Test]
    public void TransferPropertyValues_CopiesPropertiesToDestination() {
        var source = TemporaryTestDirectory.CreateJpegFile();
        var destination = TemporaryTestDirectory.CreateJpegFile();

        PropertySystem.SetPropertyValues(source, new Dictionary<PropertyDescription, object> {
            { Title, "Source Title" },
            { Keywords, new[] { "alpha", "beta" } },
        }, ignoreError: null);

        PropertySystem.TransferPropertyValues(source, destination, ignoreProperty: null, ignoreError: (kv, error) => true);

        var destinationTitle = PropertySystem.GetPropertyValue(destination, Title, throwOnError: true);
        var destinationKeywords = PropertySystem.GetPropertyValue(destination, Keywords, throwOnError: true);

        Assert.Multiple(() => {
            Assert.That(destinationTitle.Object, Is.EqualTo("Source Title"));
            Assert.That(destinationKeywords.Object, Is.EqualTo(new[] { "alpha", "beta" }));
        });
    }

    [Test]
    public void TransferPropertyValues_IgnoreProperty_SkipsFilteredProperty() {
        var source = TemporaryTestDirectory.CreateJpegFile();
        var destination = TemporaryTestDirectory.CreateJpegFile();

        PropertySystem.SetPropertyValues(source, new Dictionary<PropertyDescription, object> {
            { Title, "Source Title" },
            { Keywords, new[] { "alpha", "beta" } },
        }, ignoreError: null);

        PropertySystem.TransferPropertyValues(
            source,
            destination,
            ignoreProperty: kv => kv.Key.CanonicalName == "System.Keywords",
            ignoreError: (kv, error) => true);

        var destinationTitle = PropertySystem.GetPropertyValue(destination, Title, throwOnError: true);
        var destinationKeywords = PropertySystem.GetPropertyValue(destination, Keywords, throwOnError: true);

        Assert.Multiple(() => {
            Assert.That(destinationTitle.Object, Is.EqualTo("Source Title"));
            Assert.That(destinationKeywords.Object, Is.Null);
        });
    }
}
