using System.Collections.ObjectModel;

namespace TodoManagerConsole.Models;

public sealed class TodoList
{
    private readonly List<TodoItem> _items = new();

    public IReadOnlyList<TodoItem> Items => new ReadOnlyCollection<TodoItem>(_items);

    public void ReplaceAll(IEnumerable<TodoItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
    }

    public void Add(TodoItem item)
    {
        _items.Add(item);
    }

    public TodoItem? FindById(Guid id)
    {
        return _items.FirstOrDefault(item => item.Id == id);
    }

    public bool Remove(Guid id)
    {
        var item = FindById(id);
        if (item is null)
        {
            return false;
        }

        return _items.Remove(item);
    }
}
