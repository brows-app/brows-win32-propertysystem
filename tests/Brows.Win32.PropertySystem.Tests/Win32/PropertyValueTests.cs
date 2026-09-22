namespace Brows.Win32.Tests;

[TestFixture]
public class PropertyValueTests {
    [Test]
    public void Constructor_SetsAllProperties() {
        var description = PropertySystem.GetPropertyDescription("System.Title");
        var value = new PropertyValue(description, "display text", 42);

        Assert.Multiple(() => {
            Assert.That(value.Description, Is.SameAs(description));
            Assert.That(value.Display, Is.EqualTo("display text"));
            Assert.That(value.Object, Is.EqualTo(42));
        });
    }

    [Test]
    public void Constructor_AllowsNullDisplayAndObject() {
        var description = PropertySystem.GetPropertyDescription("System.Title");
        var value = new PropertyValue(description, null, null);

        Assert.Multiple(() => {
            Assert.That(value.Display, Is.Null);
            Assert.That(value.Object, Is.Null);
        });
    }
}
