using TodoManagerConsole.Models;
using TodoManagerConsole.Services;
using TodoManagerConsole.Storage;
using Xunit;

namespace TodoManagerConsole.Tests;

public sealed class InitializationSafetyTests
{
    [Fact]
    public async Task InitializeAsync_WhenIdsRepeat_PreservesExistingTasksAndDoesNotSave()
    {
        var id = Guid.NewGuid();
        var storage = new MemoryStorage(new[]
        {
            new TodoStorageModel { Id = id, Title = "First" },
            new TodoStorageModel { Id = id, Title = "Second" }
        });
        var list = new TodoList();
        var original = TodoItem.Create("Existing task", "Keep me");
        list.Add(original);
        var service = new TodoService(list, storage);

        await Assert.ThrowsAsync<InvalidDataException>(() => service.InitializeAsync());

        Assert.Same(original, Assert.Single(service.GetAll()));
        Assert.Equal(0, storage.SaveCount);
        Assert.Equal(2, storage.Items.Count);
    }

    [Fact]
    public async Task InitializeAsync_WhenLaterTaskIsInvalid_PreservesExistingTasks()
    {
        var storage = new MemoryStorage(new[]
        {
            new TodoStorageModel { Id = Guid.NewGuid(), Title = "Valid" },
            new TodoStorageModel { Id = Guid.NewGuid(), Title = " " }
        });
        var list = new TodoList();
        var original = TodoItem.Create("Existing task", "Keep me");
        list.Add(original);
        var service = new TodoService(list, storage);

        await Assert.ThrowsAsync<ArgumentException>(() => service.InitializeAsync());

        Assert.Same(original, Assert.Single(service.GetAll()));
        Assert.Equal(0, storage.SaveCount);
    }

    [Fact]
    public async Task InitializeAsync_WithDistinctIds_CanUpdateAndDeleteOneTask()
    {
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var storage = new MemoryStorage(new[]
        {
            new TodoStorageModel { Id = firstId, Title = "First" },
            new TodoStorageModel { Id = secondId, Title = "Second" }
        });
        var service = new TodoService(new TodoList(), storage);
        await service.InitializeAsync();

        Assert.True((await service.UpdateAsync(firstId, "Updated", "")).Success);
        Assert.True((await service.DeleteAsync(secondId)).Success);

        var remaining = Assert.Single(service.GetAll());
        Assert.Equal(firstId, remaining.Id);
        Assert.Equal("Updated", remaining.Title);
        Assert.Equal(firstId, Assert.Single(storage.Items).Id);
        Assert.Equal(2, storage.SaveCount);
    }

    private sealed class MemoryStorage : IFileStorage
    {
        public MemoryStorage(IReadOnlyList<TodoStorageModel> items) => Items = items;

        public IReadOnlyList<TodoStorageModel> Items { get; private set; }
        public int SaveCount { get; private set; }

        public Task<IReadOnlyList<TodoStorageModel>> LoadAsync() => Task.FromResult(Items);

        public Task SaveAsync(IEnumerable<TodoStorageModel> items)
        {
            Items = items.ToList();
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
