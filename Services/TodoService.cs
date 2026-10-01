using TodoManagerConsole.Models;
using TodoManagerConsole.Storage;

namespace TodoManagerConsole.Services;

public sealed class TodoService : ITodoService
{
    private readonly TodoList _todoList;
    private readonly IFileStorage _fileStorage;

    public TodoService(TodoList todoList, IFileStorage fileStorage)
    {
        _todoList = todoList;
        _fileStorage = fileStorage;
    }

    public async Task InitializeAsync()
    {
        var storedItems = await _fileStorage.LoadAsync();
        var loadedItems = storedItems.Select(item =>
            new TodoItem(item.Id, item.Title, item.Description, item.IsCompleted, item.CreatedAt, item.CompletedAt)).ToList();

        if (loadedItems.Select(item => item.Id).Distinct().Count() != loadedItems.Count)
        {
            throw new InvalidDataException("Die gespeicherten ToDos enthalten doppelte IDs. Bitte prüfe die Datei, bevor du sie erneut lädst.");
        }

        _todoList.ReplaceAll(loadedItems);
    }

    public IReadOnlyList<TodoItem> GetAll(string? searchTerm = null)
    {
        var query = _todoList.Items.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(item =>
                item.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                item.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .OrderBy(item => item.IsCompleted)
            .ThenByDescending(item => item.CreatedAt)
            .ToList();
    }

    public async Task<(bool Success, string Message)> CreateAsync(string title, string description)
    {
        var validationError = ValidateTitle(title);
        if (validationError is not null)
        {
            return (false, validationError);
        }

        var item = TodoItem.Create(title, description);
        await PersistThenReplaceAsync(_todoList.Items.Append(item));

        return (true, "ToDo wurde erstellt.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Guid id, string title, string description)
    {
        var validationError = ValidateTitle(title);
        if (validationError is not null)
        {
            return (false, validationError);
        }

        if (_todoList.FindById(id) is null)
        {
            return (false, "ToDo nicht gefunden.");
        }

        var updatedItems = CloneItems();
        updatedItems.Single(item => item.Id == id).Update(title, description);
        await PersistThenReplaceAsync(updatedItems);

        return (true, "ToDo wurde aktualisiert.");
    }

    public async Task<(bool Success, string Message)> SetCompletedAsync(Guid id, bool completed)
    {
        if (_todoList.FindById(id) is null)
        {
            return (false, "ToDo nicht gefunden.");
        }

        var updatedItems = CloneItems();
        updatedItems.Single(item => item.Id == id).SetCompleted(completed);
        await PersistThenReplaceAsync(updatedItems);

        var statusText = completed ? "erledigt" : "offen";
        return (true, $"Status wurde auf '{statusText}' gesetzt.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        if (_todoList.FindById(id) is null)
        {
            return (false, "ToDo nicht gefunden.");
        }

        await PersistThenReplaceAsync(_todoList.Items.Where(item => item.Id != id));
        return (true, "ToDo wurde gelöscht.");
    }

    private List<TodoItem> CloneItems()
    {
        return _todoList.Items.Select(item => new TodoItem(
            item.Id,
            item.Title,
            item.Description,
            item.IsCompleted,
            item.CreatedAt,
            item.CompletedAt)).ToList();
    }

    private async Task PersistThenReplaceAsync(IEnumerable<TodoItem> candidateItems)
    {
        var candidates = candidateItems.ToList();
        var payload = candidates.Select(item => new TodoStorageModel
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            IsCompleted = item.IsCompleted,
            CreatedAt = item.CreatedAt,
            CompletedAt = item.CompletedAt
        }).ToList();

        await _fileStorage.SaveAsync(payload);
        _todoList.ReplaceAll(candidates);
    }

    private static string? ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return "Titel darf nicht leer sein.";
        }

        if (title.Trim().Length > 80)
        {
            return "Titel darf maximal 80 Zeichen lang sein.";
        }

        return null;
    }
}
