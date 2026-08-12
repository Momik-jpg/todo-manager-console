using TodoManagerConsole.Models;
using TodoManagerConsole.Services;
using TodoManagerConsole.Storage;
using Xunit;

namespace TodoManagerConsole.Tests;

public sealed class PersistenceSafetyTests
{
    [Fact]
    public async Task SaveAsync_WhenEnumerationFails_PreservesExistingFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"todo-tests-{Guid.NewGuid():N}");
        var filePath = Path.Combine(directory, "todos.json");
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(filePath, "existing data");

        try
        {
            var storage = new FileStorage(filePath);

            await Assert.ThrowsAsync<InvalidOperationException>(() => storage.SaveAsync(FailingItems()));

            Assert.Equal("existing data", await File.ReadAllTextAsync(filePath));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task CreateAsync_WhenSaveFails_DoesNotChangeInMemoryList()
    {
        var list = new TodoList();
        var service = new TodoService(list, new FailingStorage());

        await Assert.ThrowsAsync<IOException>(() => service.CreateAsync("Test", ""));

        Assert.Empty(service.GetAll());
    }

    private static IEnumerable<TodoStorageModel> FailingItems()
    {
        yield return new TodoStorageModel { Id = Guid.NewGuid(), Title = "first" };
        throw new InvalidOperationException("Synthetic serialization failure");
    }

    private sealed class FailingStorage : IFileStorage
    {
        public Task<IReadOnlyList<TodoStorageModel>> LoadAsync() =>
            Task.FromResult<IReadOnlyList<TodoStorageModel>>(Array.Empty<TodoStorageModel>());

        public Task SaveAsync(IEnumerable<TodoStorageModel> items) =>
            throw new IOException("Synthetic disk failure");
    }
}
