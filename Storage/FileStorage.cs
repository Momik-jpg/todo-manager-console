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

    public string? LastRecoveryBackupPath { get; private set; }

    public async Task<IReadOnlyList<TodoStorageModel>> LoadAsync()
    {
        LastRecoveryBackupPath = null;

        if (!File.Exists(_filePath))
        {
            return Array.Empty<TodoStorageModel>();
        }

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var items = await JsonSerializer.DeserializeAsync<List<TodoStorageModel>>(stream, JsonOptions);
            return items ?? new List<TodoStorageModel>();
        }
        catch (JsonException)
        {
            LastRecoveryBackupPath = PreserveCorruptFile();
            return Array.Empty<TodoStorageModel>();
        }
    }

    public async Task SaveAsync(IEnumerable<TodoStorageModel> items)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporaryPath = $"{_filePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, items, JsonOptions);
                await stream.FlushAsync();
            }

            File.Move(temporaryPath, _filePath, overwrite: true);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }

    private string PreserveCorruptFile()
    {
        string backupPath = $"{_filePath}.corrupt-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.bak";
        File.Move(_filePath, backupPath);
        return backupPath;
    }
}
