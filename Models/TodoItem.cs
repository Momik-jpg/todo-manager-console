namespace TodoManagerConsole.Models;

public sealed class TodoItem
{
    public TodoItem(Guid id, string title, string description, bool isCompleted, DateTime createdAt, DateTime? completedAt)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title must not be empty.", nameof(title));
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        IsCompleted = isCompleted;
        CreatedAt = createdAt == default ? DateTime.UtcNow : createdAt;
        CompletedAt = completedAt;
    }

    public Guid Id { get; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public bool IsCompleted { get; private set; }

    public DateTime CreatedAt { get; }

    public DateTime? CompletedAt { get; private set; }

    public void Update(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title must not be empty.", nameof(title));
        }

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
    }

    public void SetCompleted(bool completed)
    {
        if (completed && !IsCompleted)
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
            return;
        }

        if (!completed && IsCompleted)
        {
            IsCompleted = false;
            CompletedAt = null;
        }
    }

    public static TodoItem Create(string title, string description)
    {
        return new TodoItem(Guid.NewGuid(), title, description, false, DateTime.UtcNow, null);
    }
}
