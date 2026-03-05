# Projekt 3 - ToDo-Manager (C# Console + JSON)

## Kurz erklärt
Der ToDo-Manager ist eine Console-App mit echter Datenspeicherung.  
Aufgaben werden als JSON gespeichert und beim nächsten Start wieder geladen.

## Was kann das Projekt?
- Aufgaben erstellen, bearbeiten und löschen
- Aufgaben als offen oder erledigt markieren
- Suche über Titel und Beschreibung
- automatische Speicherung in `data/todos.json`

## Projektaufbau
- `Models/`: Domänenobjekte
- `Services/`: Fachlogik und Validierung
- `Storage/`: JSON lesen/schreiben
- `Program.cs`: Menü und Ablaufsteuerung

## Start
1. `cd projects/03-todo-manager-console`
2. `dotnet run`

## Was ich gelernt habe
- klare Trennung von UI, Logik und Speicher
- Datei-Persistenz in eine laufende App integrieren
- robuste Fehlerbehandlung in Console-Projekten

## Warum im Portfolio?
Das Projekt zeigt, wie ich aus einer einfachen Übung eine alltagstaugliche Anwendung mache.
