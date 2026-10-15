using TodoManagerConsole.Storage;
using Xunit;

namespace TodoManagerConsole.Tests;

public sealed class CorruptFileRecoveryTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"todo-recovery-{Guid.NewGuid():N}");
    private string FilePath => Path.Combine(_directory, "todos.json");

    public CorruptFileRecoveryTests() => Directory.CreateDirectory(_directory);

    [Theory]
    [InlineData("{broken")]
    [InlineData("")]
    [InlineData("null")]
    [InlineData("[null]")]
    public async Task InvalidData_IsPreservedExactly_AndFreshStoreCanBeSaved(string original)
    {
        await File.WriteAllTextAsync(FilePath, original);
        var storage = new FileStorage(FilePath);

        Assert.Empty(await storage.LoadAsync());
        var backup = storage.LastRecoveryBackupPath;
        Assert.NotNull(backup);
        Assert.Equal(original, await File.ReadAllTextAsync(backup!));
        Assert.False(File.Exists(FilePath));

        await storage.SaveAsync(new[] { new TodoStorageModel { Id = Guid.NewGuid(), Title = "Recovered" } });
        var reloaded = await storage.LoadAsync();

        Assert.Equal("Recovered", Assert.Single(reloaded).Title);
        Assert.Null(storage.LastRecoveryBackupPath);
        Assert.Equal(original, await File.ReadAllTextAsync(backup!));
    }

    [Fact]
    public async Task RepeatedRecovery_CreatesSeparateBackups()
    {
        var storage = new FileStorage(FilePath);
        await File.WriteAllTextAsync(FilePath, "first damaged file");
        await storage.LoadAsync();
        var firstBackup = storage.LastRecoveryBackupPath!;
        await File.WriteAllTextAsync(FilePath, "second damaged file");
        await storage.LoadAsync();

        Assert.NotEqual(firstBackup, storage.LastRecoveryBackupPath);
        Assert.Equal("first damaged file", await File.ReadAllTextAsync(firstBackup));
        Assert.Equal("second damaged file", await File.ReadAllTextAsync(storage.LastRecoveryBackupPath!));
    }

    [Fact]
    public async Task ReadFailure_DoesNotBecomeRecovery_AndBlocksSaveUntilSuccessfulLoad()
    {
        await File.WriteAllTextAsync(FilePath, "[]");
        var storage = new FileStorage(FilePath);
        using (var lockedFile = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            await Assert.ThrowsAnyAsync<IOException>(() => storage.LoadAsync());
            Assert.Null(storage.LastRecoveryBackupPath);
            await Assert.ThrowsAsync<InvalidOperationException>(() => storage.SaveAsync(Array.Empty<TodoStorageModel>()));
        }

        Assert.Equal("[]", await File.ReadAllTextAsync(FilePath));
        Assert.Empty(await storage.LoadAsync());
        await storage.SaveAsync(Array.Empty<TodoStorageModel>());
        Assert.Empty(Directory.GetFiles(_directory, "*.corrupt-*"));
    }

    [Fact]
    public async Task MissingFile_StartsEmpty_WithoutRecoveryWarning()
    {
        var storage = new FileStorage(FilePath);
        Assert.Empty(await storage.LoadAsync());
        Assert.Null(storage.LastRecoveryBackupPath);
        Assert.Empty(Directory.GetFiles(_directory));
    }

    public void Dispose() => Directory.Delete(_directory, recursive: true);
}
