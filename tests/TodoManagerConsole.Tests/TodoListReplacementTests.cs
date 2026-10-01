using TodoManagerConsole.Models;
using Xunit;

namespace TodoManagerConsole.Tests;

public sealed class TodoListReplacementTests
{
    [Fact]
    public void ReplaceAll_WithCurrentItems_KeepsThem()
    {
        var list = new TodoList();
        var item = TodoItem.Create("Keep me", "");
        list.Add(item);

        list.ReplaceAll(list.Items);

        Assert.Same(item, Assert.Single(list.Items));
    }

    [Fact]
    public void ReplaceAll_WithDeferredFilter_MaterializesBeforeClearing()
    {
        var list = new TodoList();
        var keep = TodoItem.Create("Keep", "");
        list.Add(keep);
        list.Add(TodoItem.Create("Remove", ""));

        list.ReplaceAll(list.Items.Where(item => item.Id == keep.Id));

        Assert.Same(keep, Assert.Single(list.Items));
    }

    [Fact]
    public void ReplaceAll_WhenEnumerationFails_PreservesExistingItems()
    {
        var list = new TodoList();
        var original = TodoItem.Create("Original", "");
        list.Add(original);

        Assert.Throws<InvalidOperationException>(() => list.ReplaceAll(FailingItems()));

        Assert.Same(original, Assert.Single(list.Items));
    }

    [Fact]
    public void ReplaceAll_WithNullInput_PreservesExistingItems()
    {
        var list = new TodoList();
        var original = TodoItem.Create("Original", "");
        list.Add(original);

        Assert.Throws<ArgumentNullException>(() => list.ReplaceAll(null!));

        Assert.Same(original, Assert.Single(list.Items));
    }

    private static IEnumerable<TodoItem> FailingItems()
    {
        yield return TodoItem.Create("Partial", "");
        throw new InvalidOperationException("Simulated read failure.");
    }
}
