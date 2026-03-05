using System.Text.Json;

namespace TodoManagerConsole.Storage;

public sealed class FileStorage : IFileStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public FileStorage(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<IReadOnlyList<TodoStorageModel>> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return Array.Empty<TodoStorageModel>();
        }

        await using var stream = File.OpenRead(_filePath);
        var items = await JsonSerializer.DeserializeAsync<List<TodoStorageModel>>(stream, JsonOptions);
        return items ?? new List<TodoStorageModel>();
    }

    public async Task SaveAsync(IEnumerable<TodoStorageModel> items)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, items, JsonOptions);
    }
}
