using PaperClaw;

namespace PaperClaw.Tests;

[TestFixture]
public class ProcessedUidStoreTests
{
    private string _tempDir = string.Empty;

    [SetUp]
    public void SetUp()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Test]
    public async Task LoadAsync_MissingFile_ReturnsEmptySet()
    {
        var store = new ProcessedUidStore(_tempDir);
        var result = await store.LoadAsync();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task AddAsync_ThenLoad_RoundTrips()
    {
        var store = new ProcessedUidStore(_tempDir);
        await store.AddAsync(100u);
        await store.AddAsync(200u);

        var result = await store.LoadAsync();
        Assert.That(result, Contains.Item(100u));
        Assert.That(result, Contains.Item(200u));
    }

    [Test]
    public async Task AddAsync_NonExistentFile_CreatesIt()
    {
        var store = new ProcessedUidStore(_tempDir);
        await store.AddAsync(42u);

        var file = Path.Combine(_tempDir, ".processed-uids");
        Assert.That(File.Exists(file), Is.True);
    }

    [Test]
    public async Task LoadAsync_DuplicateUid_DeduplicatesInSet()
    {
        var file = Path.Combine(_tempDir, ".processed-uids");
        await File.WriteAllTextAsync(file, "99\n99\n99\n");

        var store = new ProcessedUidStore(_tempDir);
        var result = await store.LoadAsync();

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result, Contains.Item(99u));
    }

    [Test]
    public async Task LoadAsync_BlankLines_Ignored()
    {
        var file = Path.Combine(_tempDir, ".processed-uids");
        await File.WriteAllTextAsync(file, "\n10\n\n20\n\n");

        var store = new ProcessedUidStore(_tempDir);
        var result = await store.LoadAsync();

        Assert.That(result.Count, Is.EqualTo(2));
    }
}
