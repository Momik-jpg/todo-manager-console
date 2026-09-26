using TodoManagerConsole.Storage;
using Xunit;

namespace TodoManagerConsole.Tests;

public sealed class CorruptFileRecoveryTests
{
    [Fact]
    public async Task LoadAsync_WhenJsonIsCorrupt_MovesFileToBackupAndReturnsEmpty()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"todo-recovery-{Guid.NewGuid():N}");
        string filePath = Path.Combine(directory, "todos.json");
        const string corruptContent = "{ invalid json";

        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(filePath, corruptContent);

        try
        {
            var storage = new FileStorage(filePath);

            var items = await storage.LoadAsync();

            Assert.Empty(items);
            Assert.False(File.Exists(filePath));
            Assert.NotNull(storage.LastRecoveryBackupPath);
            Assert.Equal(corruptContent, await File.ReadAllTextAsync(storage.LastRecoveryBackupPath!));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
