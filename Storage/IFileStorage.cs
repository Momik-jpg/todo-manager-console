namespace TodoManagerConsole.Storage;

public interface IFileStorage
{
    Task<IReadOnlyList<TodoStorageModel>> LoadAsync();

    Task SaveAsync(IEnumerable<TodoStorageModel> items);
}
