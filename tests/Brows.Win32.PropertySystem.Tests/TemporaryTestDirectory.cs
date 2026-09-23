using System.IO;

namespace Brows;

/// <summary>
/// Creates one temporary directory shared by every test in this assembly, and deletes
/// it (recursively) once all tests have finished running. Individual tests create their
/// own uniquely named files inside <see cref="Root"/> to avoid interfering with each other.
/// </summary>
[SetUpFixture]
public sealed class TemporaryTestDirectory {
    public static string Root { get; private set; }

    [OneTimeSetUp]
    public void CreateRoot() {
        Root = Path.Combine(Path.GetTempPath(), "Brows.Win32.Tests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);
    }

    [OneTimeTearDown]
    public void DeleteRoot() {
        if (Root != null && Directory.Exists(Root)) {
            Directory.Delete(Root, recursive: true);
        }
    }

    /// <summary>
    /// Writes a fresh copy of the embedded test JPEG to a uniquely named file under
    /// <see cref="Root"/> and returns its full path.
    /// </summary>
    public static string CreateJpegFile() {
        var path = Path.Combine(Root, Guid.NewGuid().ToString("N") + ".jpg");
        File.WriteAllBytes(path, TestImages.TinyJpegBytes);
        return path;
    }
}
