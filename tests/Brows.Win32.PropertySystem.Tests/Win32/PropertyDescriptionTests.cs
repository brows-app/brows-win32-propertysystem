using System.Linq;

namespace Brows.Win32.Tests;

[TestFixture]
public class PropertyDescriptionTests {
    [Test]
    public void EnumeratePropertyDescriptions_NoArguments_ReturnsKnownSystemProperties() {
        var descriptions = PropertySystem.EnumeratePropertyDescriptions().ToList();

        Assert.That(descriptions, Is.Not.Empty);
        Assert.That(descriptions.Select(d => d.CanonicalName), Does.Contain("System.Title"));
        Assert.That(descriptions.Select(d => d.CanonicalName), Does.Contain("System.Size"));
    }

    [Test]
    public void EnumeratePropertyDescriptions_NoArguments_EveryItemHasCanonicalName() {
        var descriptions = PropertySystem.EnumeratePropertyDescriptions().ToList();

        Assert.That(descriptions, Is.All.Matches<PropertyDescription>(d => string.IsNullOrEmpty(d.CanonicalName) == false));
    }

    [Test]
    public void GetPropertyDescription_KnownCanonicalName_ReturnsMatchingDescription() {
        var description = PropertySystem.GetPropertyDescription("System.Title");

        Assert.Multiple(() => {
            Assert.That(description.CanonicalName, Is.EqualTo("System.Title"));
            Assert.That(description.DisplayName, Is.EqualTo("Title"));
        });
    }

    [Test]
    public void GetPropertyDescription_UnknownCanonicalName_Throws() {
        var ex = Assert.Throws<System.Runtime.InteropServices.COMException>(
            () => PropertySystem.GetPropertyDescription("Not.A.Real.Property"));

        // TYPE_E_ELEMENTNOTFOUND
        Assert.That(ex.HResult, Is.EqualTo(unchecked((int)0x8002802B)));
    }
}
