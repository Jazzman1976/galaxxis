> **Herkunft:** Mit dem Entwickler abgestimmter und am 2026-09-16 freigegebener Plan für das Refactoring der
> Basisarchitektur. Hier im Repository abgelegt, damit er bei Verlust eines lokalen Rechners erhalten bleibt.
> Der aktuelle Fortschritt steht in der Statustabelle der `CLAUDE.md`; spätere Entscheidungen während der Umsetzung
> (z. B. .NET 10 statt .NET 8, die Verbrauchs- und Mangelregeln der Wirtschaft) sind dort und in den
> Modul-READMEs dokumentiert und haben Vorrang vor diesem Dokument.

# Galaxxis: Stabile Basis — Befund und Umbauplan

## Context

Ticker, EventQueue und Processor sind die Basis von Galaxxis. Die Ausgangsfrage war, ob dieses
System stabil, ausfallsicher und gut genug für ca. 1000 gleichzeitige Spieler ist.

Die Antwort: Das Grundmuster ist richtig (zeitgesteuerte Events, ein einziger schreibender
Processor, Tick als Transaktionsgrenze, Aufholschleife). Die Umsetzung ist aber weder stabil noch
ausfallsicher und hält die Last nicht aus (Befund unten).

Darüber hinaus sind folgende Ziele festgelegt:
- **Modularer Aufbau**, damit sich Teile später einzeln skalieren und in Azure-Diensten betreiben lassen.
- **Intensive Nutzung von Dependency Injection**; Logik wird in Dienste ausgelagert, wo es sinnvoll ist.
- **Lesbarkeit vor Redundanzvermeidung**: Wiederholung ist in Ordnung, wenn sie Komplexität abbaut.
- **Datenbankzugriff neu entwerfen**: Code-First statt Scaffolding, einfach, robust, leicht verständlich.
- **Ausführliche Kommentare** im Code, damit sich die Logik ohne Lesen des Codes nachvollziehen lässt.
- **Ein Werkzeug zum Generieren neuer Universen** gehört fest zur Architektur.
- **Erweiterbar bleiben** für Features, die im Konzept noch nicht ausgearbeitet sind: **Quests**, die im
  Universum erscheinen, und **Handel zwischen Spielern** als zentrales Feature. Die Basis darf beides
  nicht verbauen. (Das Konzept nennt beides in §1.1 und §2 nur als Stichpunkt.)
- **Klares Ziel dieses Plans: Refactoring der Basisarchitektur.** Neue Spielfeatures, auch die
  fehlenden Gebäude-Oberflächen, kommen danach (siehe TODO).

---

## Entscheidungen (2026-09-16 abgestimmt)

| Thema | Entscheidung |
| --- | --- |
| Spielzeit | **Echtzeit**: Der aktuelle Tick wird aus der Uhr berechnet und läuft auch bei ausgeschaltetem Server weiter. Für Wartungen gibt es eine gezielte Pause. |
| Annahme von Befehlen | **Der Processor entscheidet verbindlich** über Kosten und laufende Baue. Web speichert nur und prüft unverbindlich für die Anzeige vor. |
| Hosting der Engine | Vorerst **im Web-Prozess** als `BackgroundService` (IIS: `AlwaysRunning`). Später als eigener Azure-Dienst startbar, ohne Code-Umbau. |
| Modulschnitt | **5 Module** + Seeder + Tests (siehe Zielarchitektur) |
| Datenbanken | **Zwei getrennt**: Accounts (Identity) und Spiel. Zwei DbContexts, beide Code-First. |
| Bestandsdaten | **Neu aufsetzen** per Migration + Seeder. Alte Datenbanken bleiben unangetastet liegen. |
| Kommentarsprache | **Englisch** (passend zu den Bezeichnern) |
| Universen | **Mehrere Universen** sind im Datenmodell und in der Engine vorgesehen (alles hängt an `UniverseId`). Ein Generator-Tool legt neue an. |
| Quests & Handel | Werden **jetzt nicht gebaut**, aber Datenmodell und Engine sind so geschnitten, dass sie ohne Umbau der Basis hinzukommen. Das wird in Schritt 4 durch Tests belegt. |
| Fehlende Gebäude-UIs | **TODO, zeitnah nach der Basis** (MageAcademy, ElementiumSynthesizer, Shipyard, DefenseSystem, Flottenbefehle) |

---

## Befund zur heutigen Tick-Engine

### Was gut ist und ins neue Design übernommen wird
- Nur ein Schreiber: Nur der Processor ändert Spielstände.
- Die Aufholschleife und `EventTick <= tick` sorgen dafür, dass nichts verpasst wird.
- Kosten werden beim Start eines Baus bezahlt (genrekorrekt).
- Event-Daten und Verhalten sind getrennt (`Event` + `IAction`).
- `GrowthRate` setzt die Konzeptformel exakt um.

### Was bricht, nach Schwere

1. **Ein fehlerhaftes Event legt das Universum dauerhaft lahm.** Ein einziges `try/catch` umschließt
   die ganze Schleife (`Processor.cs`). Wirft eine Action, gehen alle in diesem Durchlauf entnommenen
   Befehle aller Spieler verloren. Eine Sekunde später lädt der Processor dasselbe Event, wirft erneut,
   und das endlos. Die einzige Spur ist `Debug.WriteLine`, das im Release-Build wegkompiliert wird.
   Events ohne Action bzw. mit `DoAction == false` werden spurlos gelöscht.
2. **Befehle leben nur im Speicher.** Die `ConcurrentQueue` verliert ihren Inhalt bei Recycle, Absturz
   oder gestopptem Ticker. Der Rückflug nach einem Angriff wird außerhalb der Tick-Transaktion
   eingereiht; bei einem Rollback bleibt die Beute trotzdem bestehen (Duplikation).
3. **Doppelt ausgeben und doppelt bauen.** Nur der Web-Request prüft, und zwar anhand einer veralteten
   Kopie. Der Processor zieht ohne erneute Prüfung ab. Zwei parallele Klicks ergeben zwei Baue und
   negative Bestände. Zwischen `TryDequeue` und `SaveChanges` ist ein Befehl unsichtbar.
   `Shipyard.IsProducing` ist invertiert.
4. **Ressourcen werden doppelt gutgeschrieben.** `GetTicksGone` liest `PlanetDataTick` frisch aus der
   Datenbank statt von der Instanz. Mehrere Fortschreibungen im selben Tick rechnen dieselbe Zeitspanne
   mehrfach an. Ein einmal negativer Bestand erholt sich nie.
5. **Der Aufwand wächst mit der Abwesenheit.** `UpdateRessources` läuft einmal pro vergangenem Tick,
   also 86.400 Durchläufe pro Tag. Das passiert bei jedem Event und bei jedem Seitenaufruf, jeweils mit
   zusätzlichen Datenbankzugriffen (`GetTicksGone`, `BaseBuilding._owner`).
6. **Geteilter statischer Zustand zwischen Threads.** Die `PlanetCache`-Dictionaries sind ohne Sperre,
   Gebäudeobjekte hängen an fremden Planeten-Instanzen, und die Singletons werden ohne Sperre erzeugt.
7. **Zwei Uhren, die unter Last nachgehen.** `Ticker._ticks` und `UniverseTicks` laufen auseinander.
   Die Bauzeit hängt vom Server-Rückstand ab, der Timer zählt Ereignisse statt Zeit,
   `GrowthRate` nimmt fest 1 Sekunde pro Tick an, und die Reihenfolge im Tick ist zufällig.
8. **Betrieb.** Der Ticker startet nie von selbst und stirbt mit dem IIS-Idle-Timeout. Es gibt keinen
   Schutz gegen zwei Prozesse, und `.Trim()`-Abfragen erzwingen Full-Scans der Event-Tabelle.

**Last:** 1000 Spieler ergeben grob 17 Events pro Sekunde, Spitzen etwa das Zehnfache. Ein einzelner
Processor schafft das problemlos, sofern der Aufwand pro Event konstant ist. Eine Verteilung ist dafür
nicht nötig. Nötig sind Korrektheit und O(1)-Berechnungen.

---

## Zielarchitektur

### Module

```
src/
  Galaxxis.Domain/          Game rules only. No EF, no IO, no DI container.
                            GrowthRate, GameBalance (numbers from ConzeptExcel.fods), ResourceAmounts,
                            building rule classes, ResourceSettlement (O(1) formula), races.
  Galaxxis.Persistence/     Everything EF Core.
                            Game/     GameDbContext, entities, configurations, Migrations
                            Accounts/ AccountDbContext (Identity), Migrations
  Galaxxis.Application/     Services the web UI calls: submit commands, planet views,
                            onboarding, admin. Ownership checks live here.
  Galaxxis.Engine/          UniverseClock, EventProcessor, one handler per event type,
                            UniverseProcessorService (BackgroundService), health check.
  Galaxxis.Web/             Host + controllers + views. No game logic.
  Galaxxis.Tools.UniverseGenerator/
                            Console: generate a new universe (size, tick length, name, random seed),
                            bootstrap admin account from configuration. Thin host around
                            Application's IUniverseGenerationService.
  Galaxxis.Tools.LoadTest/  Console: synthetic players, measures processor lag.
tests/
  Galaxxis.Domain.Tests/    Pure unit tests (formulas vs. concept spreadsheet).
  Galaxxis.Engine.Tests/    Processor tests against SQL Server LocalDB.
```

**Erlaubte Abhängigkeiten** (in jeder `README.md` eines Moduls dokumentiert):
```
Web        → Application, Persistence (only for registration), Engine (only for registration)
Application → Domain, Persistence
Engine     → Domain, Persistence
Persistence → Domain
Domain     → nothing
```

**Innerhalb jedes Moduls wird nach Feature gegliedert, nicht nach Technik:** `Buildings/`, `Fleets/`,
`Universe/`, `Players/`. Später kommen `Quests/` und `Trading/` als eigene Ordner in Domain,
Persistence, Application, Engine und Web dazu. Ein neues Feature betrifft damit neue Ordner und keine
bestehenden Dateien.

**Azure-Tauglichkeit ergibt sich aus dem Schnitt:** Web ist zustandslos und kann per App Service
horizontal skaliert werden. Die Engine läuft heute im Web-Host und später als eigener Container App
oder WebJob. Der neue Host ruft dann nur `AddGalaxxisPersistence()` + `AddGalaxxisEngine()` auf.
Konfiguration, Logging und Health Checks nutzen die Standardschnittstellen, sodass Key Vault,
Application Insights und Azure SQL später nur zusätzliche Provider sind.

### Dependency Injection — Regeln
- Jedes Modul hat genau eine Registrierungs-Methode: `services.AddGalaxxisDomain()`,
  `AddGalaxxisPersistence(configuration)`, `AddGalaxxisApplication()`, `AddGalaxxisEngine(configuration)`.
  `Program.cs` setzt die Anwendung daraus zusammen.
- **Interfaces** gibt es für Dienste, die Modulgrenzen überqueren oder im Test ersetzt werden
  (Application-Dienste, Event-Handler, Uhr). **Keine** Interfaces für reine Regelklassen und Records.
- **Kein generisches Repository-Pattern.** Der `DbContext` ist bereits Unit of Work. Dienste nutzen ihn
  direkt, das ist einfacher zu lesen.
- Lebensdauern: `DbContext` scoped (Web); `IDbContextFactory<GameDbContext>` in der Engine (ein Context
  pro Event); Handler und Dienste scoped; Uhr singleton (`TimeProvider` aus .NET 8, im Test `FakeTimeProvider`).
- Konfiguration über das Options-Pattern mit `ValidateDataAnnotations().ValidateOnStart()`. Eine falsche
  Konfiguration verhindert den Start, statt später zur Laufzeit aufzufallen.
- Keine statischen Provider, keine Singletons per `static Instance`, kein `Activator.CreateInstance`.

### Datenbank — Code-First, zwei Contexts

**`GameDbContext`** (Datenbank `GalaxxisGame`):

| Entity | Wesentliches |
| --- | --- |
| `Universe` | `Name`, `EpochUtc`, `TickLength`, `PausedSinceUtc?`, `TotalPausedDuration` |
| `Galaxy`, `StarSystem`, `Planet`, `Moon` | Koordinaten. `StarSystem` statt `System`, weil `System` mit dem Namespace kollidiert. |
| `Planet` | `OwnerPlayerId?` — **`null` = unbesetzt** (ersetzt den „Nobody"-User mit Id 2). Owned Types `Resources` (7 Werte) und `Buildings` (Stufe + Auslastung je Gebäude): in der Datenbank flache Spalten, im Code gruppiert. `ResourcesSettledTick`, `RowVersion`. |
| `Player` | `AccountId` (Identity-User-Id, eindeutig; ersetzt die Verknüpfung per E-Mail), `Race` |
| `Ship`, `Defense` | wie heute, aufgeräumt |
| `GameEvent` | `UniverseId` (Pflicht), `Type` (String, kein festes Enum in der DB), `State` (`Requested → Scheduled → Done`, oder `Rejected`/`Failed`), `PlayerId?` (leer bei System-Events), `PlanetId?` (leer, wenn nicht planetgebunden), `RequestedTick`, `ProcessAtTick`, `PayloadJson`, `Error`, `CreatedUtc`, `CompletedUtc` |

`PlayerId` und `PlanetId` sind bewusst **optional**. Ein Event ist nicht an einen Spieler oder Planeten
gebunden, sondern nur an ein Universum. Alles Weitere steht im typisierten Payload. Genau das brauchen
Quests (vom System ausgelöst, an Koordinaten im Raum) und Handel (zwei Spieler, zwei Planeten).

- Enums werden als **Strings** gespeichert und sind damit in der Datenbank direkt lesbar.
- Payload als **JSON** (`System.Text.Json`) mit einem Record pro Event-Typ, statt XML + Reflection.
- Das bisherige `EventLog` entfällt: Erledigte Events bleiben mit `State=Done` als Historie stehen.
- Indizes: `(State, ProcessAtTick, Id)` für den Processor. **Gefilterte eindeutige Indizes**
  „höchstens ein aktiver Bau" bzw. „höchstens eine aktive Produktion" pro Planet. Die Datenbank
  selbst verhindert so Doppelbaue.
- Eine `IEntityTypeConfiguration<T>` pro Entity, jede kommentiert.

**`AccountDbContext`** (Datenbank `GalaxxisAccounts`): `IdentityDbContext<AppUser, AppRole, string>`,
eigene Migrations-Historie, Rolle `Admin`.

**Migrations — das Prinzip in drei Sätzen** (steht so auch in `Galaxxis.Persistence/README.md`):
Das Schema wird durch die Entity-Klassen und Konfigurationen im Code definiert. Jede Schemaänderung
wird mit `dotnet ef migrations add <Name> --context GameDbContext` als Migration festgehalten und
eingecheckt. In der Entwicklung werden Migrations beim Start automatisch angewendet
(`Database:ApplyMigrationsOnStartup=true`), für Azure wird ein idempotentes SQL-Skript erzeugt
(`dotnet ef migrations script --idempotent`).

### Engine — Ablauf

```
Web-Request → Application.SubmitCommand
  ├─ Vorprüfung (nur für die Meldung „nicht genug Erz")
  └─ INSERT GameEvent (State=Requested, RequestedTick=ProcessAtTick=clock.CurrentTick)

UniverseProcessorService (BackgroundService, PeriodicTimer)
  ├─ hält sp_getapplock → zweite Instanz wartet als Hot-Standby (Redundanz)
  └─ EventProcessor: lädt IDs, State in (Requested, Scheduled) AND ProcessAtTick <= CurrentTick,
     ORDER BY ProcessAtTick, Id. Für jedes Event: eigener DbContext + Transaktion
       ├─ Planet bis ProcessAtTick fortschreiben (O(1), nie bis „jetzt")
       ├─ Requested → Handler.Accept: prüfen, Kosten abziehen, ProcessAtTick += Dauer → Scheduled | Rejected
       ├─ Scheduled → Handler.Complete: Wirkung anwenden, Folge-Events per context.Add → Done
       └─ Exception → Rollback, State=Failed + Error, ILogger, weiter mit dem nächsten Event
```
- `UniverseClock.CurrentTick = floor((UtcNow − EpochUtc − TotalPausedDuration) / TickLength)`.
- Leere Ticks kosten nichts: Verarbeitet wird nur, was fällig ist. Nach einem stundenlangen Ausfall ist
  der Aufwand proportional zur Zahl der Events, nicht zur Zahl der Ticks.
- Health Check `/health`: Rückstand (ältestes überfälliges Event), Zeitpunkt des letzten erfolgreichen
  Durchlaufs, Lock gehalten ja/nein.
- **Mehrere Universen:** Der Service verarbeitet jedes laufende Universum mit eigener Uhr und eigenem
  `sp_getapplock` (`galaxxis-universe-{id}`). Universen sind voneinander isoliert, und später könnte
  jedes auf einer eigenen Engine-Instanz laufen.

### Universum-Generator

- **Algorithmus in Domain** (`Galaxxis.Domain/Universe/Generation/`): 3D-Koordinaten, Namen aus Präfix
  und Suffix, Mondwahrscheinlichkeit. Alles rein, ohne Datenbank, gesteuert über einen **Random-Seed**.
  Derselbe Seed ergibt dasselbe Universum, das ist reproduzierbar und testbar. Die bisherige Logik aus
  `Galaxxis.Data/Program.cs` und `UniverseCreator.cs` wird übernommen und aufgeräumt.
- **`UniverseGenerationOptions`**: Name, Anzahl Galaxien, Systeme pro Galaxie, Planeten pro System,
  Mondchancen, Ticklänge, Seed. Standardwerte kommen aus der Konfiguration, einzelne Werte lassen sich
  per Kommandozeile überschreiben.
- **`IUniverseGenerationService` in Application** schreibt ein Universum in **einer Transaktion**
  (ganz oder gar nicht) und startet es pausiert, damit es vor der Freigabe geprüft werden kann.
  Weil der Dienst in Application liegt, kann ihn später auch eine Admin-Seite im Web aufrufen.
- **`Galaxxis.Tools.UniverseGenerator`** ist die Konsole dazu:
  `generate --name Andromeda --galaxies 2 --seed 42`, `list`, `resume <id>`, `bootstrap-admin`.
  Zusätzlich gibt es einen interaktiven Modus wie heute.

### Erweiterbarkeit für Quests und Handel

Nichts davon wird jetzt gebaut. Die Basis stellt aber sicher, dass beides **ohne Änderung an
Processor, bestehenden Handlern oder bestehenden Tabellen** hinzukommen kann:

| Bedarf | Wie die Basis ihn abdeckt |
| --- | --- |
| Quests erscheinen von selbst im Universum | **System-Events** ohne `PlayerId`. Ein wiederkehrendes Event (z. B. `SpawnQuests`) plant in `Complete` transaktional seine nächste Ausführung selbst ein. Dafür ist keine zusätzliche Scheduler-Infrastruktur nötig. |
| Quests an Orten im Raum statt auf Planeten | `Coordinate` als Value Object in Domain; `GameEvent.PlanetId` ist optional. |
| Handel: Ware sicher reservieren | Dasselbe Muster wie Baukosten: **`Accept` reserviert** (zieht beim Anbieter ab), **`Complete` überträgt**, Abbruch/Ablauf erstattet. Alles innerhalb der Transaktion pro Event. |
| Handel: zwei Planeten zweier Spieler in einem Schritt | Der Processor schreibt **alle beteiligten Planeten** bis zum Event-Tick fort; die Transaktion pro Event umfasst beliebig viele Zeilen; `RowVersion` auf Planet sichert zusätzlich ab. |
| Neue Tabellen (`Quest`, `TradeOffer`, …) | Neuer Feature-Ordner + neue `IEntityTypeConfiguration` + Migration. Bestehende Tabellen bleiben unberührt. |
| Neue Event-Typen | `Type` ist ein String; Handler werden per DI **automatisch gefunden** (Assembly-Scan nach `IGameEventHandler`). Der Processor muss nicht geändert werden. |
| Spieler über Ergebnisse informieren | Die `GameEvent`-Historie je Spieler (`State`, `Error`, Payload) ist die Grundlage für ein späteres Ereignis-Postfach. |
| Punkte für Handel und Quests (Konzept §2) | Punkte werden in Handlern vergeben, dafür gibt es eine eigene `PlayerScore`-Tabelle, sobald sie gebraucht wird. |

**Belegt wird die Erweiterbarkeit in Schritt 4 durch zwei Test-Handler**, die nur im Testprojekt
existieren: ein selbst wiederkehrendes System-Event ohne Spieler und Planet sowie eine Übertragung von
Ressourcen zwischen zwei Planeten zweier Spieler mit Reservierung. Beide müssen ohne Änderung an der
Engine laufen.

### Kommentar-Konventionen
- Englisch. **XML-Dokumentation an jedem öffentlichen Typ und Member.** Das wird über
  `GenerateDocumentationFile` erzwungen, ein fehlender Kommentar erzeugt die Warnung CS1591.
- Die Klassen-Summary beantwortet: Wofür ist die Klasse da, wer ruft sie auf, in welches Modul gehört sie.
- Inline-Kommentare erklären Spielregeln und nicht offensichtliche Entscheidungen. Sie verweisen auf das
  Konzept, z. B. `// Concept: KonzeptWord §6 growth rates`.
- Jedes Projekt hat eine kurze `README.md` mit Zweck, erlaubten Abhängigkeiten und Einstiegspunkten.
- Diese Konvention ersetzt für Galaxxis die sonst sparsame Kommentierung und wird in `CLAUDE.md` festgehalten.

### Lesbarkeit vor Redundanzvermeidung — konkret
- Ein Handler pro Event-Typ als eigene Klasse, auch wenn sich Handler ähneln. Keine generische
  Reflection-Maschinerie.
- Ein Gebäude = eine Regelklasse mit expliziten Zahlen und Konzeptverweis.
- Wo Wiederholung **nur mechanisch** ist (7× derselbe Block für 7 Ressourcen in `UpdateRessources`
  und `FleetCommandAttackAction`), fasst der Value Object `ResourceAmounts` sie zusammen. Das dient
  hier der Lesbarkeit.

---

## Schritte, der Reihe nach

Gearbeitet wird auf einem Branch `feature/stable-foundation` (von `develop`). Nach jedem Schritt baut
die Solution grün, die Tests laufen, und es folgt ein Commit nach deiner Freigabe. Die alten Projekte
`Galaxxis.Core` und `Galaxxis.Data` bleiben bis Schritt 5 unverändert lauffähig.

### Schritt 1 — Fundament
- Die neuen Projekte (siehe Module) werden angelegt und in `src/Galaxxis.sln` aufgenommen, jeweils
  mit `README.md` und Registrierungs-Methode.
- `global.json` pinnt das .NET 10 SDK. `Directory.Build.props`: `net10.0` (LTS bis 11/2028; .NET 8
  endet 11/2026, am 2026-09-16 so entschieden), `Nullable=enable`, `ImplicitUsings`,
  `GenerateDocumentationFile`, Warnungen für fehlende Dokumentation. Die Einstellungen gelten nur für
  die neuen Projekte; die Altprojekte bleiben bis Schritt 5 unverändert.
- `Directory.Packages.props`: zentrale Paketversionen, EF Core und Identity auf 10.x.
- Konfiguration: `appsettings.json` ohne Geheimnisse, eingecheckte `appsettings.Local.Template.json`
  mit Platzhaltern (Connection-Strings, Bootstrap-Admin, Universe-Optionen), die echte
  `appsettings.Local.json` steht in `.gitignore`. Options-Klassen mit Validierung.
- `ILogger` überall; Grundgerüst für den Health Check.
- Test-Projekte (xUnit) angelegt.
- `CLAUDE.md`: Modulregeln, Feature-Ordner, DI-Regeln, Kommentar-Konventionen, Migrations-Prinzip,
  Erweiterungspunkte für neue Features.
- `docs/plans/TODO.md` mit der TODO-Liste (siehe unten) anlegen.

**Fertig, wenn:** Die Solution baut, leere Tests laufen, Web startet wie bisher.

### Schritt 2 — Domain: Spielregeln herauslösen
- `GrowthRate` wird übernommen. Die Umrechnung pro Tick nutzt künftig die Ticklänge statt fest 1 Sekunde.
- `GameBalance` ersetzt `Constants`: Alle Zahlen aus `ConzeptExcel.fods`, jede mit Konzeptverweis.
- `ResourceAmounts` (7 Ressourcen, Rechenoperationen), `ResourceType`, `Race` mit Boni und Mali
  laut KonzeptWord §5 für alle sechs Rassen.
- Gebäude-Regelklassen ohne Datenbankzugriff und ohne Cache, erzeugt aus einem Planeten-Zustand
  (Stufen, Auslastung, Rasse).
- `ResourceSettlement.Settle(...)`: geschlossene Formel, Bestand nie unter 0, positive Raten erholen
  auch einen Bestand von 0.
- `Coordinate` (Value Object) und der **Generator-Algorithmus** (`Universe/Generation/`) mit Seed.
- Tests: `GrowthRate` gegen das Tabellenblatt „Wachstums Raten" (z. B. Konstruktion normal, Stufe 20 =
  21.675). Die Formel wird gegen die alte Schleife geprüft (als Referenz im Test nachgebaut), plus
  explizite Tests für das korrigierte Verhalten bei negativem Bestand. Der Generator liefert bei gleichem
  Seed dasselbe Universum; Namen sind eindeutig; die Mondverteilung liegt im erwarteten Rahmen.

**Fertig, wenn:** Alle Spielformeln sind rein, dokumentiert und gegen das Konzept getestet.

### Schritt 3 — Persistence: Code-First
- Die Entities und Konfigurationen aus „Datenbank" oben werden angelegt, dazu `GameDbContext` und
  `AccountDbContext`.
- Initial-Migrationen für beide Contexts, dazu ein automatisches Anwenden beim Start, das sich per
  Option abschalten lässt.
- `IUniverseGenerationService` (Application) und **`Galaxxis.Tools.UniverseGenerator`** ersetzen
  `Galaxxis.Data/Program.cs` und `UniverseCreator.cs`: Universum in einer Transaktion generieren,
  auflisten, fortsetzen; Admin-Account + Rolle aus der Konfiguration anlegen. Aufruf per Kommandozeile
  oder interaktiv (siehe „Universum-Generator").

**Fertig, wenn:** Aus leerem SQL Server entstehen per Migration + `UniverseGenerator` beide Datenbanken
mit spielbarem Universum und Admin. Ein zweites Universum lässt sich daneben anlegen.

### Schritt 4 — Engine
- `UniverseClock` (Echtzeit + Pause) auf Basis von `TimeProvider`.
- `IGameEventHandler` mit `Accept` / `Complete`, ein Handler pro vorhandenem Event-Typ (9 Baue,
  7 Auslastungsänderungen, 3 Produktionen, 3 Flottenbefehle). Übernommen wird das heutige Verhalten,
  aber korrekt: Prüfung im Handler, Folge-Events transaktional. Registrierung per DI, doppelte
  Registrierung bricht den Start ab.
- `EventProcessor` + `UniverseProcessorService` mit `sp_getapplock`, Fehlerisolation pro Event,
  deterministischer Reihenfolge und Health Check.
- Tests mit LocalDB und `FakeTimeProvider`: Poison-Event, 20 parallele Bauanfragen, Rollback verwirft
  Folge-Events, Fortschreibung bis zum Event-Tick, Aufholen nach simuliertem Ausfall, zweite Instanz
  verarbeitet nicht, zwei Universen laufen isoliert nebeneinander.
- **Erweiterbarkeits-Tests** mit zwei Handlern, die nur im Testprojekt existieren (siehe „Erweiterbarkeit"):
  ein wiederkehrendes System-Event ohne Spieler und Planet sowie eine reservierte
  Ressourcenübertragung zwischen zwei Spielern. Beide laufen ohne jede Änderung an Engine oder Schema.

**Fertig, wenn:** Alle Engine-Tests sind grün, und die Engine verarbeitet Events ohne Web-UI (über den Seeder/Tests angestoßen).

### Schritt 5 — Application + Web umstellen
- Application-Dienste: `ICommandService` (Befehle einreichen und Status abfragen), `IPlanetViewService`
  (Planeten- und Ressourcenansicht, nur zur Anzeige fortgeschrieben), `IOnboardingService`
  (genau einmal einen zufälligen freien Planeten zuweisen, beim Betreten des Universums),
  `IUniverseAdminService` (Pause/Fortsetzen, Status).
- **Besitzprüfung zentral in Application**: Jeder Befehl nimmt den Spieler aus `ICurrentPlayer`,
  nie aus dem Request.
- Web: `ICurrentPlayer` (Claims → `AccountId` → `Player`), durchgehend async, globaler
  Antiforgery-Filter, `[Authorize(Roles="Admin")]` für Admin, Login nur per POST mit Lockout.
- Entfernen: `HomeController`/`Home.cshtml` samt Links, `Account/AddAdmin`,
  die sofortige Planetenzuweisung in `Account/SelectUniverse`, `Galaxxis.Core`, `Galaxxis.Data`,
  `src/Backup/Databases.zip` (wird durch die Migrationen überflüssig).
- Gebäude-Controller (OreMine, CrystalMine, Factory, PowerSupply, NeutronAccelerator) laufen über die
  Application-Dienste. Das AJAX-Muster von OreMine zeigt „beauftragt → gestartet/abgelehnt".
- **Bewusste Folge:** MageAcademy, ElementiumSynthesizer, Shipyard, DefenseSystem und Flottenbefehle
  haben bis zum ersten Feature-Schritt nach der Basis keine Oberfläche mehr (bisher nur über Home
  erreichbar). Die Engine-Handler dafür existieren. Das steht als TODO mit hoher Priorität in
  `docs/plans/TODO.md` (siehe unten).

**Fertig, wenn:** Die Spielkette Login → Universe → Galaxy → System → Planet → Gebäude bauen funktioniert
vollständig auf der neuen Basis. Audit-Befunde A1–A8, B1, B2 und C1–C10 sind erledigt.

### Schritt 6 — Abnahme „Basis stabil"
- Lasttest mit `Galaxxis.Tools.LoadTest` und die Ausfallübungen aus „Verifikation" durchführen,
  Ergebnisse festhalten.
- Audit-Dokument mit Erledigt-Status aktualisieren; `CLAUDE.md` und Modul-READMEs auf Endstand bringen.

### TODO — nach der Basis (wird in Schritt 1 als `docs/plans/TODO.md` angelegt)

In dieser Reihenfolge:

1. **Zeitnah: fehlende Gebäude-Oberflächen** nach dem OreMine-Muster: MageAcademy,
   ElementiumSynthesizer, Shipyard (Schiffsproduktion), DefenseSystem (Verteidigungsproduktion),
   Flottenbefehle (Angriff, Scan, Bewegung). Die Handler in der Engine existieren bereits.
2. **Handel zwischen Spielern** als zentrales Feature. Vor der Umsetzung muss das Konzept ausgearbeitet
   werden: Angebotsarten, Gebühren, Transportweg, Punkte laut §2.
3. **Quests**, die im Universum erscheinen. Auch hier zuerst das Konzept: Arten, Belohnungen, Spawn-Regeln.
4. **Kampfsystem** laut KonzeptWord §7.
5. **Azure-Deployment**: App Service, Azure SQL, Engine als eigener Dienst, Key Vault, Application
   Insights, Data-Protection-Keys im Blob Storage für mehrere Web-Instanzen.
6. Admin-Seite im Web zum Generieren und Freigeben von Universen (nutzt `IUniverseGenerationService`).

---

## Kritische Dateien

| Heute | Wird zu |
| --- | --- |
| `src/Galaxxis.Core/Engine/Processor.cs`, `Ticker.cs` | `Galaxxis.Engine/Processing/EventProcessor.cs`, `UniverseProcessorService.cs`, `Time/UniverseClock.cs` |
| `src/Galaxxis.Core/Actions/*.cs` | `Galaxxis.Engine/Handlers/*Handler.cs` |
| `src/Galaxxis.Core/Classes/GrowthRate.cs`, `Static/Constants.cs`, `Classes/Buildings/**` | `Galaxxis.Domain/…` (ohne DB, ohne Cache) |
| `src/Galaxxis.Core/Extensions/PlanetExtensions/PlanetCache.*.cs` | entfällt; `Galaxxis.Domain/Resources/ResourceSettlement.cs` |
| `src/Galaxxis.Core/Provider/*.cs` | Application-Dienste bzw. direkte `DbContext`-Nutzung |
| `src/Galaxxis.Data/Models/*`, `Events/*`, `EventTypeData/*` | `Galaxxis.Persistence/Game/Entities/*`, `GameEvent` + Payload-Records |
| `src/Galaxxis.Data/Program.cs`, `UniverseCreator.cs` | `Galaxxis.Domain/Universe/Generation/*` + `Galaxxis.Application/Universe/UniverseGenerationService.cs` + `Galaxxis.Tools.UniverseGenerator` |
| `src/Galaxxis.Web/Code/Identity/*`, `Migrations/*` | `Galaxxis.Persistence/Accounts/*` |
| `src/Galaxxis.Web/Startup.cs`, `Program.cs` | `Program.cs` mit Minimal-Hosting und Modul-Registrierungen |
| `src/Galaxxis.Web/Code/Services/CurrentUserService/*` | `ICurrentPlayer` |
| `src/Galaxxis.Web/Views/*/…Controller.cs`, `…ViewModel.cs` | nutzen Application-Dienste; `Home` entfällt |

---

## Verifikation

- **Unit** (`dotnet test`): Formeln gegen die Konzepttabelle; geschlossene Formel = alte Schleife;
  korrigiertes Verhalten bei negativem Bestand.
- **Poison-Event**: Ein Event mit kaputtem Payload wird angelegt. Erwartet: Es landet auf `Failed` mit
  Fehlertext, alle anderen Events werden verarbeitet, der Health Check zeigt keinen Rückstand.
- **Doppelklick**: 20 parallele Bauanfragen für denselben Planeten. Erwartet: genau 1 `Scheduled`,
  19 `Rejected`, Kosten einmal abgezogen.
- **Neustart**: Befehle absenden, den Prozess hart beenden, neu starten. Erwartet: Kein Befehl fehlt,
  das Universum läuft ohne Admin-Klick weiter, die Ausfallzeit wurde mit Produktion abgerechnet.
- **Zwei Instanzen**: Zwei Web-Prozesse gegen dieselbe Datenbank. Erwartet: Nur einer verarbeitet.
  Wird er beendet, übernimmt der andere innerhalb eines Timer-Intervalls.
- **Pause**: Pausieren, 10 Minuten warten, fortsetzen. Erwartet: keine Produktion und kein Baufortschritt
  in dieser Zeit.
- **Last**: 1000 synthetische Spieler, 50 Befehle pro Sekunde, 30 Minuten, ein Teil mit tagelang nicht
  fortgeschriebenen Planeten. Erwartet: Rückstand bleibt nahe 0, Verarbeitungszeit pro Event im
  einstelligen Millisekundenbereich, keine doppelten Stufen, keine negativen Bestände.
- **Frischer Aufbau**: Leerer SQL Server → Web starten (Migrationen) →
  `UniverseGenerator bootstrap-admin` → `UniverseGenerator generate --seed 42` → `resume <id>` →
  Login als Admin → registrieren → Universum betreten → Planet bekommen → Erzmine bauen → nach der
  Bauzeit Stufe 1.
- **Generator**: Zweimal mit demselben Seed generieren ergibt identische Koordinaten und Namen. Ein
  Abbruch mitten in der Generierung hinterlässt kein halbes Universum (Transaktion).
- **Erweiterbarkeit**: Die beiden Test-Handler (System-Event, Übertragung zwischen Spielern) laufen
  grün, ohne dass Engine, bestehende Handler oder Schema geändert wurden.
