using System.Text.Json;

namespace TodoManagerConsole.Storage;

public sealed class FileStorage : IFileStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;
    private bool _loadFailed;

    public string? LastRecoveryBackupPath { get; private set; }

    public FileStorage(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<IReadOnlyList<TodoStorageModel>> LoadAsync()
    {
        LastRecoveryBackupPath = null;
        _loadFailed = true;
        try
        {
            if (!File.Exists(_filePath))
            {
                _loadFailed = false;
                return Array.Empty<TodoStorageModel>();
            }

            await using var stream = File.OpenRead(_filePath);
            var items = await JsonSerializer.DeserializeAsync<List<TodoStorageModel>>(stream, JsonOptions);
            if (items is null || items.Any(item => item is null))
            {
                throw new JsonException("Die ToDo-Datei muss eine Liste mit gültigen Einträgen enthalten.");
            }

            _loadFailed = false;
            return items;
        }
        catch (JsonException)
        {
            // The read stream has been disposed before moving the file (also on Windows).
            var backupPath = $"{_filePath}.corrupt-{DateTime.UtcNow:yyyyMMddTHHmmssfffZ}-{Guid.NewGuid():N}.json";
            File.Move(_filePath, backupPath, overwrite: false);
            LastRecoveryBackupPath = backupPath;
            _loadFailed = false;
            return Array.Empty<TodoStorageModel>();
        }
    }

    public async Task SaveAsync(IEnumerable<TodoStorageModel> items)
    {
        if (_loadFailed)
        {
            throw new InvalidOperationException("Speichern ist nach einem Ladefehler gesperrt. Die Originaldatei bleibt erhalten.");
        }

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
}
