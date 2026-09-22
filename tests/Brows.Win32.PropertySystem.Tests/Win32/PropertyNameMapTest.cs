using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Brows.Win32;

[TestFixture]
internal sealed class PropertyNameMapTest {
    [Test]
    public void GetCanonicalNames_DuplicateLegacyName_ReturnsEveryDocumentedCanonicalName() {
        Assert.That(PropertyNameMap.GetCanonicalNames("Copyright"), Is.EqualTo(new[] { "System.Copyright", "System.Image.Copyright" }));
        Assert.That(PropertyNameMap.GetCanonicalNames("Status"), Is.EqualTo(new[] { "System.Media.Status", "System.Status" }));
    }

    [Test]
    public void GetCanonicalNames_LegacyNameRepeatedWithSameCanonicalName_ReturnsSingleEntry() {
        Assert.That(PropertyNameMap.GetCanonicalNames("Compression"), Is.EqualTo(new[] { "System.Video.Compression" }));
    }

    [Test]
    public void GetCanonicalNames_UnambiguousLegacyName_ReturnsSingleEntry() {
        Assert.That(PropertyNameMap.GetCanonicalNames("WhenTaken"), Is.EqualTo(new[] { "System.Photo.DateTaken" }));
    }

    [Test]
    public void GetCanonicalNames_CanonicalName_MapsToItself() {
        Assert.That(PropertyNameMap.GetCanonicalNames("System.Image.Copyright"), Is.EqualTo(new[] { "System.Image.Copyright" }));
    }

    [Test]
    public void GetCanonicalNames_UnknownName_ReturnsNull() {
        Assert.That(PropertyNameMap.GetCanonicalNames("Not.A.Real.Property"), Is.Null);
    }

    [Test]
    public void GetCanonicalName_DuplicateLegacyName_ReturnsNameWindowsResolves() {
        Assert.That(PropertyNameMap.GetCanonicalName("Copyright"), Is.EqualTo("System.Copyright"));
        Assert.That(PropertyNameMap.GetCanonicalName("Status"), Is.EqualTo("System.Media.Status"));
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
                mismatched.Add($"{legacyName}: windows={description.CanonicalName}, map=[{string.Join(", ", canonicalNames)}]");
            }
        }
        Assert.That(mismatched, Is.Empty);
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
