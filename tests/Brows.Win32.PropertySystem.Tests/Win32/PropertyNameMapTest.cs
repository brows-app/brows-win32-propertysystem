using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Brows.Win32;

[TestFixture]
internal sealed class PropertyNameMapTest {
    [Test]
    public void GetCanonicalNames_DuplicateLegacyName_ReturnsEveryDocumentedCanonicalName() {
        using (Assert.EnterMultipleScope()) {
            Assert.That(PropertyNameMap.GetCanonicalNames("Copyright"),
                    Is.EqualTo(["System.Copyright", "System.Image.Copyright"]));
            Assert.That(PropertyNameMap.GetCanonicalNames("Status"),
                Is.EqualTo(["System.Media.Status", "System.Status"]));
        }
    }

    [Test]
    public void GetCanonicalNames_LegacyNameRepeatedWithSameCanonicalName_ReturnsSingleEntry() {
        Assert.That(PropertyNameMap.GetCanonicalNames("Compression"),
            Is.EqualTo(["System.Video.Compression"]));
    }

    [Test]
    public void GetCanonicalNames_UnambiguousLegacyName_ReturnsSingleEntry() {
        Assert.That(PropertyNameMap.GetCanonicalNames("WhenTaken"),
            Is.EqualTo(["System.Photo.DateTaken"]));
    }

    [Test]
    public void GetCanonicalNames_CanonicalName_ReturnsNull() {
        Assert.That(PropertyNameMap.GetCanonicalNames("System.Image.Copyright"), Is.Null);
    }

    [Test]
    public void GetCanonicalNames_UnknownName_ReturnsNull() {
        Assert.That(PropertyNameMap.GetCanonicalNames("Not.A.Real.Property"), Is.Null);
    }

    [Test]
    public void GetCanonicalNames_UnknownNameAndThrowIfNotFound_Throws() {
        Assert.That(
            () => PropertyNameMap.GetCanonicalNames("Not.A.Real.Property", throwIfNotFound: true),
            Throws.ArgumentException);
    }

    [Test]
    public void GetCanonicalNames_KnownNameAndThrowIfNotFound_DoesNotThrow() {
        Assert.That(
            PropertyNameMap.GetCanonicalNames("WhenTaken", throwIfNotFound: true),
            Is.EqualTo(new[] { "System.Photo.DateTaken" }));
    }

    [Test]
    public void GetCanonicalName_UnknownNameAndThrowIfNotFound_Throws() {
        Assert.That(
            () => PropertyNameMap.GetCanonicalName("Not.A.Real.Property", throwIfNotFound: true),
            Throws.ArgumentException);
    }

    [Test]
    public void GetCanonicalName_KnownNameAndThrowIfNotFound_DoesNotThrow() {
        Assert.That(
            PropertyNameMap.GetCanonicalName("WhenTaken", throwIfNotFound: true),
            Is.EqualTo("System.Photo.DateTaken"));
    }

    [Test]
    public void GetCanonicalName_CanonicalName_ReturnsNull() {
        Assert.That(PropertyNameMap.GetCanonicalName("System.Photo.DateTaken"), Is.Null);
    }

    [Test]
    public void GetCanonicalName_DuplicateLegacyName_ReturnsNameWindowsResolves() {
        using (Assert.EnterMultipleScope()) {
            Assert.That(PropertyNameMap.GetCanonicalName("Copyright"), Is.EqualTo("System.Copyright"));
            Assert.That(PropertyNameMap.GetCanonicalName("Status"), Is.EqualTo("System.Media.Status"));
        }
    }

    [Test]
    public void GetCanonicalName_UnambiguousLegacyName_ReturnsCanonicalName() {
        Assert.That(PropertyNameMap.GetCanonicalName("DocTitle"), Is.EqualTo("System.Title"));
    }

    [Test]
    public void GetCanonicalName_UnknownName_ReturnsNull() {
        Assert.That(PropertyNameMap.GetCanonicalName("Not.A.Real.Property"), Is.Null);
    }

    [Test]
    public void GetCanonicalName_EveryLegacyNameWindowsKnows_AgreesWithWindows() {
        var mismatched = new List<string>();
        foreach (var legacyName in LegacyNames) {
            PropertyDescription description;
            try {
                description = PropertySystem.GetPropertyDescription(legacyName);
            }
            catch (COMException) {
                continue;
            }
            if (description == null) {
                continue;
            }
            var canonicalNames = PropertyNameMap.GetCanonicalNames(legacyName);
            if (canonicalNames.Contains(description.CanonicalName) == false) {
                mismatched.Add(
                    $"{legacyName}: windows={description.CanonicalName}, map=[{string.Join(", ", canonicalNames)}]");
            }
        }
        Assert.That(mismatched, Is.Empty);
    }

    [Test]
    public void GetCanonicalNames_LegacyNameWithDifferentCasing_Matches() {
        var expected = new[] { "System.Photo.DateTaken" };
        using (Assert.EnterMultipleScope()) {
            Assert.That(PropertyNameMap.GetCanonicalNames("WhenTaken"), Is.EqualTo(expected));
            Assert.That(PropertyNameMap.GetCanonicalNames("whentaken"), Is.EqualTo(expected));
            Assert.That(PropertyNameMap.GetCanonicalNames("WHENTAKEN"), Is.EqualTo(expected));
            Assert.That(PropertyNameMap.GetCanonicalNames("wHeNtAkEn"), Is.EqualTo(expected));
        }
    }

    [Test]
    public void GetCanonicalNames_LegacyNameWithSpaceAndDifferentCasing_Matches() {
        Assert.That(PropertyNameMap.GetCanonicalNames("audio format"),
            Is.EqualTo(["System.Audio.Format"]));
    }

    [Test]
    public void GetCanonicalNames_DuplicateLegacyNameWithDifferentCasing_Matches() {
        Assert.That(PropertyNameMap.GetCanonicalNames("cOpYrIgHt"),
            Is.EqualTo(["System.Copyright", "System.Image.Copyright"]));
    }

    [Test]
    public void GetCanonicalName_LegacyNameWithDifferentCasing_ReturnsCanonicalCasing() {
        Assert.That(PropertyNameMap.GetCanonicalName("doctitle"),
            Is.EqualTo("System.Title"));
    }

    [Test]
    public void GetCanonicalName_NullName_Throws() {
        Assert.That(() => PropertyNameMap.GetCanonicalName(null),
            Throws.ArgumentNullException);
    }

    [Test]
    public void GetCanonicalNames_NullName_Throws() {
        Assert.That(() => PropertyNameMap.GetCanonicalNames(null),
            Throws.ArgumentNullException);
    }

    [Test]
    public void GetCanonicalName_UpperCasedLegacyName_RoundTripsThroughWindows() {
        foreach (var legacyName in LegacyNames) {
            var canonicalName = PropertyNameMap.GetCanonicalName(legacyName.ToUpperInvariant());
            using (Assert.EnterMultipleScope()) {
                Assert.That(canonicalName, Is.Not.Null, $"'{legacyName}' did not match case-insensitively.");
                Assert.That(
                    PropertySystem.GetPropertyDescription(canonicalName).CanonicalName,
                    Is.EqualTo(canonicalName),
                    $"'{canonicalName}' was not accepted by Windows.");
            }
        }
    }

    private static IEnumerable<string> LegacyNames => [
        "Access", "Album", "AllocSize", "Aperture", "Artist", "Attrib", "Attributes",
        "AttributesDescription", "BitDepth", "Bitrate", "CameraModel", "Capacity", "Channels",
        "ColorSpace", "Company", "Compression", "Copyright", "Create", "CSCStatus", "DateDeleted",
        "DeletedFrom", "Dimensions", "Directory", "Distance", "DocAppName", "DocAuthor",
        "DocKeywords", "DocSubject", "DocTitle", "Duration", "EquipMake", "FileSystem",
        "FileVersion", "Flash", "FNumber", "FocalLength", "FrameCount", "FreeSpace", "Genre",
        "ImageX", "ImageY", "ISOSpeed", "LinkTarget", "Lyrics", "Manager", "Name", "Owner",
        "Protected", "Rank", "Rating", "ResolutionX", "ResolutionY", "Scale", "ShutterSpeed",
        "Size", "Software", "Status", "SyncCopyIn", "Track", "Type", "WhenTaken", "Write", "Year"
    ];
}
