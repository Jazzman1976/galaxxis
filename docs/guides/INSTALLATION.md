# Installationsanleitung

Diese Anleitung führt von einem leeren Rechner bis zu einem laufenden Spiel mit eigenem Universum, Admin-Zugang und
erstem Spieler. Sie richtet sich an Entwickler und an alle, die Galaxxis lokal ausprobieren wollen. Für einen Betrieb
auf einem Server oder auf Azure gelten zusätzlich die Hinweise im Abschnitt [Server-Betrieb](#server-betrieb).

Alle Befehle werden, wenn nicht anders angegeben, im **Repository-Root** ausgeführt (dort, wo `Galaxxis.slnx` liegt).

## Überblick

| Schritt | Ergebnis |
| --- | --- |
| 1. [Voraussetzungen](#1-voraussetzungen) installieren | .NET 10 SDK, SQL Server (LocalDB), Git |
| 2. [Quellcode holen und bauen](#2-quellcode-holen-und-bauen) | Lauffähige Projekte |
| 3. [Lokale Konfiguration anlegen](#3-lokale-konfiguration-anlegen) | `appsettings.Local.json` mit Datenbanken und Admin |
| 4. [Datenbanken und Admin anlegen](#4-datenbanken-und-admin-anlegen) | Datenbanken `GalaxxisGame` und `GalaxxisAccounts`, Admin-Account |
| 5. [Universum erzeugen und starten](#5-universum-erzeugen-und-starten) | Ein laufendes Universum |
| 6. [Spiel starten und beitreten](#6-spiel-starten-und-beitreten) | Spiel im Browser, erster Spieler mit Heimatplanet |

Kurzfassung für Ungeduldige (Windows mit Visual Studio):

```
git clone https://github.com/Jazzman1976/galaxxis.git
cd galaxxis
dotnet tool restore
copy appsettings.Local.json.template appsettings.Local.json      # Admin-Passwort eintragen
dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- bootstrap-admin
dotnet run --project src/Galaxxis.Web                            # https://localhost:5001
```

Danach als Admin anmelden, unter `/admin` ein Universum erzeugen und starten, mit einem zweiten Account registrieren
und beitreten.

## 1. Voraussetzungen

| Was | Version | Hinweis |
| --- | --- | --- |
| **.NET SDK** | 10.0.401 oder neuer im selben Feature-Band (siehe `global.json`) | <https://dotnet.microsoft.com/download/dotnet/10.0>. Prüfen: `dotnet --version` |
| **SQL Server** | LocalDB, Express, Developer oder ein Docker-Container | Standard ist **SQL Server LocalDB** (Windows). Sie wird mit Visual Studio installiert, sonst über den Installer von SQL Server Express (Option „LocalDB“). Prüfen: `sqllocaldb info` |
| **Git** | beliebig | Zum Klonen des Repositorys |
| Entwicklungsumgebung | optional | Visual Studio 2026, oder VS Code mit den Erweiterungen *C# Dev Kit* und *C#* |
| Node.js | optional | Nur nötig, wenn SCSS- oder JavaScript-Quellen geändert werden (siehe [Oberfläche ändern](#oberfläche-ändern)) |

Ohne Windows (Linux, macOS) gibt es keine LocalDB. Dann einen SQL-Server-Container verwenden, siehe
[Anderer Datenbankserver](#anderer-datenbankserver).

Für HTTPS auf `localhost` braucht .NET ein Entwicklungszertifikat. Einmalig vertrauen:

```
dotnet dev-certs https --trust
```

## 2. Quellcode holen und bauen

```
git clone https://github.com/Jazzman1976/galaxxis.git
cd galaxxis
dotnet tool restore          # installiert das lokale Werkzeug dotnet-ef (dotnet-tools.json)
dotnet build
```

Der Build muss ohne Fehler durchlaufen. Warnungen zu Nullable-Referenzen gelten als Fehler
(`Directory.Build.props`); taucht hier ein Fehler auf, passt meist die SDK-Version nicht zu `global.json`.

## 3. Lokale Konfiguration anlegen

Alle rechnerspezifischen Einstellungen (Verbindungen, Passwörter) stehen in **`appsettings.Local.json`** im
Repository-Root. Die Datei ist von Git ausgeschlossen, damit nie ein Passwort eingecheckt wird. Als Vorlage dient die
eingecheckte `appsettings.Local.json.template`, die jede Einstellung ausführlich erklärt.

```
copy appsettings.Local.json.template appsettings.Local.json      # Windows
cp appsettings.Local.json.template appsettings.Local.json        # Linux, macOS, Git Bash
```

Dann `appsettings.Local.json` öffnen und anpassen:

| Einstellung | Was eintragen |
| --- | --- |
| `ConnectionStrings:Game`, `ConnectionStrings:Accounts` | Mit LocalDB passen die Vorgaben, nichts ändern. Zwei **verschiedene** Datenbanken sind Pflicht. |
| `Persistence:ApplyMigrationsOnStartup` | `true` lassen: Dann legt das erste Programm, das startet, beide Datenbanken samt Schema an. |
| `BootstrapAdmin:Email`, `UserName` | Adresse und Anmeldename des ersten Administrators. Der Name darf keine Leerzeichen enthalten. |
| `BootstrapAdmin:Password` | **Pflicht.** Mindestens 6 Zeichen, mit Klein- und Großbuchstabe, Ziffer und Sonderzeichen, z. B. `Galaxxis-Admin-2026!` |

Beim Bauen kopiert jedes Programm (Web, Werkzeuge) die Datei automatisch neben seine ausführbare Datei. Die
Reihenfolge der Quellen, jede spätere überschreibt die frühere: `appsettings.json` des Projekts →
`appsettings.Local.json` → Umgebungsvariablen (`:` wird zu `__`, z. B. `ConnectionStrings__Game`) → Kommandozeile.

Fehlt eine Pflichteinstellung oder ist sie ungültig, startet kein Programm. Stattdessen erscheint eine Meldung, die
sagt, welcher Wert fehlt und wie er auszusehen hat.

## 4. Datenbanken und Admin anlegen

```
dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- bootstrap-admin
```

Beim ersten Lauf entstehen die Datenbanken `GalaxxisGame` und `GalaxxisAccounts` (über die EF-Core-Migrationen),
danach die Rolle `Admin` und der Account aus `BootstrapAdmin`. Der Befehl kann gefahrlos wiederholt werden; gibt es
den Account schon, bekommt er nur die Rolle.

Ein Admin-Account wird bewusst nicht über die Weboberfläche angelegt. Nach diesem Schritt kann das Passwort in
`appsettings.Local.json` wieder geleert werden; eine spätere Änderung dort ändert das Passwort des bestehenden
Accounts nicht.

## 5. Universum erzeugen und starten

Es gibt zwei gleichwertige Wege.

**Im Browser (Admin-Seite):** Spiel starten (Schritt 6), mit dem Admin anmelden und `/admin` öffnen. Dort ein
Universum erzeugen (Name, Seed, Größe und Tick-Länge lassen sich ändern) und anschließend **starten**.

**Mit dem Konsolenwerkzeug:**

```
dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- generate --name Andromeda --seed 42
dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- list          # zeigt die Id des Universums
dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- resume 1      # Id aus der Liste
```

Wichtig: **Ein neues Universum ist pausiert.** Erst nach dem Starten (`resume` bzw. „Starten“ auf `/admin`) läuft die
Universumszeit, wird produziert und werden Bauaufträge fertig. Die Zeit läuft nach der Uhr weiter, auch wenn das Spiel
gerade nicht läuft; beim nächsten Start wird alles nachgeholt. Zum Anhalten dient `pause <id>`.

Ohne Befehl startet das Werkzeug ein interaktives Menü; `help` listet alle Befehle und Optionen. Die Standardkarte
hat 10 Galaxien × 10 Sternsysteme × 10 Planeten. Derselbe Seed mit denselben Karteneinstellungen erzeugt immer
dieselbe Karte. Details: [`src/Galaxxis.Tools.UniverseGenerator/README.md`](../../src/Galaxxis.Tools.UniverseGenerator/README.md).

## 6. Spiel starten und beitreten

```
dotnet run --project src/Galaxxis.Web
```

Das Spiel läuft unter **<https://localhost:5001>** (HTTP: `http://localhost:5000`). Die Spiel-Engine läuft im selben
Prozess mit; solange die Webanwendung läuft, werden Ereignisse verarbeitet.

1. Mit einem **neuen Account registrieren** (für das Spielen nicht den Admin verwenden).
2. Auf der Startseite das Universum wählen und **beitreten**: Rasse wählen, der Heimatplanet wird zufällig unter
   den freien Planeten zugeteilt.
3. Auf dem Planeten ein Rohstoffgebäude ausbauen. Nach Ablauf des Countdowns muss die neue Stufe erscheinen. Dann
   arbeiten Engine und Datenbank richtig zusammen.

In **VS Code** genügt F5 mit der Konfiguration `Galaxxis.Web` (`.vscode/launch.json`); sie öffnet auch den Browser.
In **Visual Studio** `Galaxxis.slnx` öffnen und das Profil `Galaxxis.Web` starten.

## Installation prüfen

| Prüfung | Erwartung |
| --- | --- |
| `https://localhost:5001/health` | `Healthy`: Die Engine verarbeitet Ereignisse. Direkt nach dem Start kurz `Degraded`, bis der erste Durchlauf fertig ist; `Unhealthy` heißt, die Engine hängt (Log prüfen). |
| `dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- list` | Das Universum steht auf `running`, nicht `paused`. |
| `dotnet test` | Alle Tests grün. Die Tests legen eigene, temporäre Datenbanken auf LocalDB an und löschen sie danach wieder; die Spieldatenbanken bleiben unberührt. Anderer Server: siehe [`tests/README.md`](../../tests/README.md). |

## Anderer Datenbankserver

Jeder SQL Server ab 2019 funktioniert, auch Azure SQL. Nur die beiden Verbindungszeichenfolgen in
`appsettings.Local.json` ändern; Beispiele für SQL Express, SQL-Login und Azure SQL stehen in der Vorlage.

Beispiel für einen **Docker-Container** (auch unter Linux und macOS):

```
docker run -d --name galaxxis-sql -e ACCEPT_EULA=Y -e "MSSQL_SA_PASSWORD=<eigenes Passwort>" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

```jsonc
"ConnectionStrings": {
  "Game": "Server=localhost,1433;Database=GalaxxisGame;User Id=sa;Password=<eigenes Passwort>;TrustServerCertificate=True",
  "Accounts": "Server=localhost,1433;Database=GalaxxisAccounts;User Id=sa;Password=<eigenes Passwort>;TrustServerCertificate=True"
}
```

Das Passwort des Containers muss den Regeln von SQL Server genügen (mindestens 8 Zeichen aus drei der vier Gruppen
Großbuchstaben, Kleinbuchstaben, Ziffern, Sonderzeichen), sonst beendet sich der Container sofort. Der Login braucht
das Recht, Datenbanken anzulegen, solange `ApplyMigrationsOnStartup` die Datenbanken erzeugen soll.

## Server-Betrieb

Für einen Rechner, auf dem das Spiel dauerhaft läuft, gilt zusätzlich:

- **Veröffentlichen:** `dotnet publish src/Galaxxis.Web -c Release -o dist/web` (oder das Profil `FolderProfile`, das
  ebenfalls nach `dist/web` schreibt). `appsettings.Local.json` gehört nicht ins Paket; auf dem Server die Werte als
  Umgebungsvariablen oder, bei Azure App Service, als „Application settings“ und „Connection strings“ setzen.
- **Migrationen:** `Persistence:ApplyMigrationsOnStartup` auf `false` und das Schema vorab per idempotentem
  SQL-Skript einspielen (`dotnet ef migrations script --idempotent …`, genaue Befehle in
  [`src/Galaxxis.Persistence/README.md`](../../src/Galaxxis.Persistence/README.md)).
- **Nur eine Engine:** Die Engine läuft im Webprozess und hält eine Sperre in der Datenbank, damit nie zwei Engines
  dasselbe Universum verarbeiten. Weitere Webinstanzen bekommen `Engine:Enabled = false`
  ([`src/Galaxxis.Engine/README.md`](../../src/Galaxxis.Engine/README.md)).
- **IIS:** Der Anwendungspool darf nicht in den Leerlauf gehen (Startmodus `AlwaysRunning`, Leerlauf-Timeout 0),
  sonst verarbeitet die Engine nichts, solange niemand die Seite aufruft. Details in
  [`src/Galaxxis.Web/README.md`](../../src/Galaxxis.Web/README.md), Abschnitt „IIS“.
- **Überwachung:** `/health` ist ohne Anmeldung erreichbar und eignet sich für Health-Checks.

## Oberfläche ändern

Die kompilierten CSS- und JavaScript-Dateien sind eingecheckt; zum Bauen des Spiels ist kein Node.js nötig. Nur nach
Änderungen an `wwwroot/styles/src` oder `wwwroot/scripts/src`:

```
cd src/Galaxxis.Web
npm install          # einmalig
npx gulp all
```

## Häufige Probleme

| Problem | Ursache und Lösung |
| --- | --- |
| Start bricht mit einer Meldung zu `ConnectionStrings` oder `BootstrapAdmin` ab | `appsettings.Local.json` fehlt, liegt nicht im Repository-Root oder ein Pflichtwert ist leer. Datei anlegen (Schritt 3) und neu bauen, damit sie kopiert wird. |
| `bootstrap-admin` meldet ein ungültiges Passwort | Das Passwort erfüllt die Regeln nicht (Groß-, Kleinbuchstabe, Ziffer, Sonderzeichen, mindestens 6 Zeichen). |
| Keine Verbindung zu `(localdb)\MSSQLLocalDB` | LocalDB ist nicht installiert oder die Instanz fehlt. `sqllocaldb info` prüfen, notfalls `sqllocaldb create MSSQLLocalDB` und `sqllocaldb start MSSQLLocalDB`. In JSON den Backslash doppelt schreiben: `(localdb)\\MSSQLLocalDB`. |
| Browser warnt vor dem Zertifikat | Entwicklungszertifikat fehlt: `dotnet dev-certs https --trust`. |
| Bauaufträge werden nie fertig, Rohstoffe steigen nicht | Das Universum ist noch pausiert (`resume <id>` oder „Starten“ auf `/admin`), oder die Webanwendung läuft nicht, sodass keine Engine arbeitet. |
| `/admin` antwortet mit „Zugriff verweigert“ | Der angemeldete Account hat die Rolle `Admin` nicht. `bootstrap-admin` mit dessen E-Mail-Adresse ausführen und neu anmelden. |
| Der Build meldet eine fehlende oder falsche SDK-Version | Das .NET 10 SDK aus `global.json` installieren. |

## Neu anfangen

Für einen kompletten Neustart die beiden Datenbanken löschen, zum Beispiel mit SQL Server Management Studio oder:

```
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "DROP DATABASE GalaxxisGame; DROP DATABASE GalaxxisAccounts"
```

Danach wieder bei [Schritt 4](#4-datenbanken-und-admin-anlegen) beginnen. Alte Spielstände werden nicht übernommen.
