using TodoManagerConsole.Models;

namespace TodoManagerConsole.Services;

public interface ITodoService
{
    Task InitializeAsync();

    IReadOnlyList<TodoItem> GetAll(string? searchTerm = null);

    Task<(bool Success, string Message)> CreateAsync(string title, string description);

    Task<(bool Success, string Message)> UpdateAsync(Guid id, string title, string description);

    Task<(bool Success, string Message)> SetCompletedAsync(Guid id, bool completed);

    Task<(bool Success, string Message)> DeleteAsync(Guid id);
}
