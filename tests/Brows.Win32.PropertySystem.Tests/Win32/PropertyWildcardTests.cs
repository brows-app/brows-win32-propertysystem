namespace Brows.Win32;

[TestFixture]
public class PropertyWildcardTests {
    [Test]
    public void Constructor_NullName_ThrowsArgumentNullException() {
        Assert.Throws<ArgumentNullException>(() => new PropertyWildcard(null));
    }

    [Test]
    public void Name_ReturnsConstructorValue() {
        var wildcard = new PropertyWildcard("System.Photo.Aperture");
        Assert.That(wildcard.Name, Is.EqualTo("System.Photo.Aperture"));
    }

    [Test]
    public void Equality_SameName_ReturnsEqual() {
        var first = new PropertyWildcard("System.Photo.Aperture");
        var second = new PropertyWildcard("System.Photo.Aperture");

        Assert.That(first, Is.EqualTo(second));
    }

    [Test]
    public void Equality_DifferentName_ReturnsNotEqual() {
        var first = new PropertyWildcard("System.Photo.Aperture");
        var second = new PropertyWildcard("System.Photo.FNumber");

        Assert.That(first, Is.Not.EqualTo(second));
    }

    [Test]
    public void GetHashCode_SameName_ReturnsSameValue() {
        var first = new PropertyWildcard("System.Photo.Aperture");
        var second = new PropertyWildcard("System.Photo.Aperture");

        Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
    }

    [Test]
    public void ToString_ContainsRecordTypeAndName() {
        var wildcard = new PropertyWildcard("System.Photo.Aperture");

        Assert.That(wildcard.ToString(), Does.StartWith("PropertyWildcard {").And.Contain("Name = System.Photo.Aperture"));
    }

    [Test]
    public void Parts_SplitsNameByDot() {
        var wildcard = new PropertyWildcard("System.Photo.Aperture");
        Assert.That(wildcard.Parts, Is.EqualTo(new[] { "System", "Photo", "Aperture" }));
    }

    [Test]
    public void Parts_SingleSegmentName_ReturnsOneElement() {
        var wildcard = new PropertyWildcard("System");
        Assert.That(wildcard.Parts, Is.EqualTo(new[] { "System" }));
    }

    [Test]
    public void Matches_NullCandidate_ReturnsFalse() {
        var wildcard = new PropertyWildcard("System.Title");
        Assert.That(wildcard.Matches(null), Is.False);
    }

    [Test]
    public void Matches_IdenticalName_ReturnsTrue() {
        var wildcard = new PropertyWildcard("System.Title");
        Assert.That(wildcard.Matches("System.Title"), Is.True);
    }

    [Test]
    public void Matches_DifferentCase_ReturnsTrue() {
        var wildcard = new PropertyWildcard("SYSTEM.TITLE");
        Assert.That(wildcard.Matches("system.title"), Is.True);
    }

    [Test]
    public void Matches_DifferentSegmentValue_ReturnsFalse() {
        var wildcard = new PropertyWildcard("System.Photo");
        Assert.That(wildcard.Matches("System.Video"), Is.False);
    }

    [Test]
    public void Matches_CandidateWithFewerSegmentsThanPattern_ReturnsFalse() {
        var wildcard = new PropertyWildcard("System.Photo.Aperture");
        Assert.That(wildcard.Matches("System.Photo"), Is.False);
    }

    [Test]
    public void Matches_TrailingWildcard_MatchesLongerCandidate() {
        var wildcard = new PropertyWildcard("System.Photo.*");
        Assert.That(wildcard.Matches("System.Photo.Aperture"), Is.True);
    }

    [Test]
    public void Matches_TrailingWildcard_MatchesExactPrefixAlone() {
        var wildcard = new PropertyWildcard("System.Photo.*");
        Assert.That(wildcard.Matches("System.Photo"), Is.True);
    }

    [Test]
    public void Matches_BareWildcard_MatchesAnyCandidate() {
        var wildcard = new PropertyWildcard("*");
        Assert.That(wildcard.Matches("Anything.Deeply.Nested"), Is.True);
    }

    [Test]
    public void Matches_CandidateWithMoreSegmentsAndNoWildcard_ReturnsFalse() {
        var wildcard = new PropertyWildcard("System.Photo");
        Assert.That(wildcard.Matches("System.Photo.EXIF.DateTaken"), Is.False);
    }

    [Test]
    public void Matches_CandidateWithOneExtraSegmentAndNoWildcard_ReturnsFalse() {
        var wildcard = new PropertyWildcard("System.Photo");
        Assert.That(wildcard.Matches("System.Photo.Aperture"), Is.False);
    }

    [Test]
    public void Matches_TrailingWildcard_MatchesDeeplyNestedCandidate() {
        var wildcard = new PropertyWildcard("System.Photo.*");
        Assert.That(wildcard.Matches("System.Photo.EXIF.DateTaken"), Is.True);
    }
}
