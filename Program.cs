using TodoManagerConsole.Models;
using TodoManagerConsole.Services;
using TodoManagerConsole.Storage;

var appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
var storagePath = Path.Combine(appDataDirectory, "TodoManagerConsole", "todos.json");
var fileStorage = new FileStorage(storagePath);
ITodoService todoService = new TodoService(new TodoList(), fileStorage);

try
{
    await todoService.InitializeAsync();
    if (fileStorage.LastRecoveryBackupPath is { } backupPath)
    {
        Console.Error.WriteLine($"Die beschädigte ToDo-Datei wurde gesichert: {backupPath}");
        Console.Error.WriteLine("Der ToDo-Manager startet mit einer leeren Liste.");
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Die gespeicherten ToDos konnten nicht geladen werden: {ex.Message}");
    return;
}

var shouldRun = true;

while (shouldRun)
{
    Console.WriteLine("=== ToDo-Manager (OOP + JSON) ===");
    Console.WriteLine("1) Alle ToDos anzeigen");
    Console.WriteLine("2) ToDos suchen");
    Console.WriteLine("3) ToDo erstellen");
    Console.WriteLine("4) ToDo bearbeiten");
    Console.WriteLine("5) ToDo Status wechseln");
    Console.WriteLine("6) ToDo löschen");
    Console.WriteLine("7) Beenden");
    Console.WriteLine();

    Console.Write("Auswahl: ");
    var choice = Console.ReadLine();

    if (choice is null)
    {
        Console.WriteLine("Eingabe beendet.");
        break;
    }

    Console.WriteLine();

    try
    {
        switch (choice)
        {
            case "1":
                PrintTodos(todoService);
                Pause();
                break;
            case "2":
                SearchTodos(todoService);
                Pause();
                break;
            case "3":
                await CreateTodoAsync(todoService);
                Pause();
                break;
            case "4":
                await EditTodoAsync(todoService);
                Pause();
                break;
            case "5":
                await ToggleTodoStatusAsync(todoService);
                Pause();
                break;
            case "6":
                await DeleteTodoAsync(todoService);
                Pause();
                break;
            case "7":
                shouldRun = false;
                Console.WriteLine("Programm beendet.");
                break;
            default:
                Console.WriteLine("Bitte wähle eine Option von 1 bis 7.");
                Pause();
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fehler: {ex.Message}");
        Pause();
    }

    if (shouldRun)
    {
        Console.Clear();
    }
}

static void SearchTodos(ITodoService service)
{
    Console.Write("Suchbegriff: ");
    var search = Console.ReadLine();

    var filteredItems = service.GetAll(search);

    if (filteredItems.Count == 0)
    {
        Console.WriteLine("Keine passenden ToDos gefunden.");
        return;
    }

    PrintTodoList(filteredItems);
}

static async Task CreateTodoAsync(ITodoService service)
{
    Console.Write("Titel: ");
    var title = Console.ReadLine() ?? string.Empty;

    Console.Write("Beschreibung (optional): ");
    var description = Console.ReadLine() ?? string.Empty;

    var result = await service.CreateAsync(title, description);
    Console.WriteLine(result.Message);
}

static async Task EditTodoAsync(ITodoService service)
{
    var selectedId = SelectTodoId(service, "bearbeiten");

    if (selectedId is null)
    {
        return;
    }

    Console.Write("Neuer Titel: ");
    var title = Console.ReadLine() ?? string.Empty;

    Console.Write("Neue Beschreibung: ");
    var description = Console.ReadLine() ?? string.Empty;

    var result = await service.UpdateAsync(selectedId.Value, title, description);
    Console.WriteLine(result.Message);
}

static async Task ToggleTodoStatusAsync(ITodoService service)
{
    var allItems = service.GetAll();

    if (allItems.Count == 0)
    {
        Console.WriteLine("Keine ToDos vorhanden.");
        return;
    }

    PrintTodoList(allItems);
    Console.WriteLine();

    Console.Write("Nummer des ToDos für Statuswechsel: ");

    if (!TryGetSelection(allItems.Count, out var index))
    {
        Console.WriteLine("Ungültige Nummer.");
        return;
    }

    var selected = allItems[index];
    var newStatus = !selected.IsCompleted;

    var result = await service.SetCompletedAsync(selected.Id, newStatus);
    Console.WriteLine(result.Message);
}

static async Task DeleteTodoAsync(ITodoService service)
{
    var selectedId = SelectTodoId(service, "löschen");

    if (selectedId is null)
    {
        return;
    }

    var result = await service.DeleteAsync(selectedId.Value);
    Console.WriteLine(result.Message);
}

static Guid? SelectTodoId(ITodoService service, string action)
{
    var allItems = service.GetAll();

    if (allItems.Count == 0)
    {
        Console.WriteLine("Keine ToDos vorhanden.");
        return null;
    }

    PrintTodoList(allItems);
    Console.WriteLine();

    Console.Write($"Nummer des ToDos zum {action}: ");

    if (!TryGetSelection(allItems.Count, out var index))
    {
        Console.WriteLine("Ungültige Nummer.");
        return null;
    }

    return allItems[index].Id;
}

static bool TryGetSelection(int maxCount, out int selectedIndex)
{
    selectedIndex = -1;

    var input = Console.ReadLine();

    if (!int.TryParse(input, out var number))
    {
        return false;
    }

    if (number < 1 || number > maxCount)
    {
        return false;
    }

    selectedIndex = number - 1;
    return true;
}

static void PrintTodos(ITodoService service)
{
    var allItems = service.GetAll();

    if (allItems.Count == 0)
    {
        Console.WriteLine("Noch keine ToDos erfasst.");
        return;
    }

    PrintTodoList(allItems);
}

static void PrintTodoList(IReadOnlyList<TodoItem> items)
{
    for (var i = 0; i < items.Count; i++)
    {
        var item = items[i];
        var status = item.IsCompleted ? "Erledigt" : "Offen";
        var suffix = string.IsNullOrWhiteSpace(item.Description) ? string.Empty : $" | {item.Description}";

        Console.WriteLine($"{i + 1}. [{status,-8}] {item.Title}{suffix}");
    }
}

static void Pause()
{
    Console.WriteLine();
    Console.WriteLine("Enter drücken zum Fortfahren...");
    Console.ReadLine();
}
