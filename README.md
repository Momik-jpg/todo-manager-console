# ToDo Manager

## Kurz erklärt
Der ToDo-Manager ist eine Console-App mit echter Datenspeicherung.  
Aufgaben werden atomar als JSON im lokalen Anwendungsordner gespeichert und beim nächsten Start wieder geladen.

## Was kann das Projekt?
- Aufgaben erstellen, bearbeiten und löschen
- Aufgaben als offen oder erledigt markieren
- Suche über Titel und Beschreibung
- automatische Speicherung unter `%LOCALAPPDATA%/TodoManagerConsole/todos.json`
- Schutz der bestehenden Datei, falls ein Speichervorgang fehlschlägt

## Projektaufbau
- `Models/`: Domänenobjekte
- `Services/`: Fachlogik und Validierung
- `Storage/`: JSON lesen/schreiben
- `Program.cs`: Menü und Ablaufsteuerung

## Start
```powershell
dotnet run --project 03-todo-manager-console.csproj
```

## Tests

```powershell
dotnet test tests/TodoManagerConsole.Tests/TodoManagerConsole.Tests.csproj
```

## Was ich gelernt habe
- klare Trennung von UI, Logik und Speicher
- Datei-Persistenz in eine laufende App integrieren
- Fehlerbehandlung und atomare Dateispeicherung in Console-Projekten

## Warum im Portfolio?
Das Projekt zeigt, wie ich aus einer einfachen Übung eine alltagstaugliche Anwendung mache.
