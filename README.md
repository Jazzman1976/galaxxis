# Galaxxis

[![Build](https://github.com/Jazzman1976/galaxxis/actions/workflows/build.yml/badge.svg?branch=develop)](https://github.com/Jazzman1976/galaxxis/actions/workflows/build.yml)

Browserbasiertes Weltraum-Strategiespiel im Stil von OGame: Spieler besitzen Planeten, bauen Gebäude aus, produzieren
Schiffe und Verteidigung und treten gegen andere Spieler an. ASP.NET Core MVC auf .NET 10, serverseitig gerenderte
Razor-Seiten, SQL Server.

## Schnellstart

Voraussetzungen: .NET 10 SDK (siehe `global.json`), SQL Server LocalDB.

```
dotnet tool restore
copy appsettings.Local.json.template appsettings.Local.json     # dann das Admin-Passwort eintragen
dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- bootstrap-admin
dotnet run --project src/Galaxxis.Web                           # https://localhost:5001
```

Als Admin anmelden und unter `/admin` ein Universum erzeugen und starten; danach mit einem zweiten Account
registrieren und dem Universum beitreten. Tests: `dotnet test` im Repository-Root (braucht LocalDB).
In VS Code genügt F5 (Konfigurationen in `.vscode/launch.json`).

## Wo was steht

| Ort | Inhalt |
| --- | --- |
| [`CLAUDE.md`](CLAUDE.md) | Architektur, Projektaufbau, Build und die Regeln für neuen Code (Englisch) |
| [`docs/`](docs/README.md) | Spielkonzept, freigegebene Pläne, TODO-Liste, Prüfberichte |
| `src/<Projekt>/README.md` | Zweck, Abhängigkeiten und Konfiguration jedes Moduls |
| `appsettings.Local.json.template` | Alle Einstellungen mit Erklärung; Kopie ohne `.template` ist die lokale Konfiguration |
| `dist/` | Ausgabe des Publish-Profils (`dist/web`, nicht eingecheckt) |
