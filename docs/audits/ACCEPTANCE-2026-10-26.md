# Galaxxis — Abnahme „Basis stabil“ (Schritt 6)

**Stand:** 2026-10-26 · **Branch:** `feature/stable-foundation`

Schritt 6 des Basis-Umbaus (`docs/plans/FOUNDATION-REFACTORING-PLAN.md`) verlangt den Lasttest und die
Ausfallübungen aus dem Abschnitt „Verifikation“ und das Festhalten der Ergebnisse. Dieses Dokument ist dieser
Nachweis. Der Erledigt-Status der Audit-Befunde steht in [`AUDIT-2026-09-16.md`](AUDIT-2026-09-16.md).

**Ergebnis: Die Basis ist abgenommen.** Alle Punkte der Verifikation sind erfüllt, entweder in einem Lauf am
2026-10-26 (Lasttest, harter Neustart, Generator) oder durch automatisierte Tests, die bei jedem `dotnet test`
erneut laufen. Ein Punkt (frischer Aufbau im Browser) ist durch Tests abgedeckt, ein Durchlauf von Hand steht aus,
siehe unten.

---

## Übersicht

| Verifikation (Plan) | Erwartet | Nachweis | Ergebnis |
| --- | --- | --- | --- |
| Unit | Formeln = Konzepttabelle, geschlossene Formel = alte Schleife, negativer Bestand korrigiert | `Galaxxis.Domain.Tests` | erfüllt |
| Poison-Event | Kaputter Payload → `Failed` mit Fehlertext, andere Events laufen weiter | `FailureIsolationTests` (4 Tests); im Lasttest 0 `Failed` | erfüllt |
| Doppelklick | 20 parallele Bauanfragen → 1 `Scheduled`, 19 `Rejected`, Kosten einmal | `BuildingEventTests.Of_twenty_simultaneous_build_requests_…`; im Lasttest 6 solche Fälle korrekt abgelehnt | erfüllt |
| Neustart | Nach hartem Beenden fehlt kein Befehl, Universum läuft ohne Admin weiter, Ausfallzeit mit Produktion abgerechnet | Übung am 2026-10-26 (unten); `TimeAndInstanceTests.After_an_outage_…`, `BuildingEventTests.A_late_processed_construction_…` | erfüllt |
| Zwei Instanzen | Nur eine verarbeitet, die andere übernimmt innerhalb eines Intervalls | `TimeAndInstanceTests.Only_one_engine_instance_processes_a_universe_and_a_standby_takes_over` (zwei echte Engine-Instanzen, echte `sp_getapplock`-Sperre) | erfüllt |
| Pause | Pausiert keine Produktion, kein Baufortschritt | `TimeAndInstanceTests.A_paused_universe_processes_nothing_…` (ein Tag Pause über die Test-Uhr: der Ausbau braucht danach noch volle 25 s). Produktion hängt an derselben Universumszeit (`UniverseTimeline`), die in der Pause steht | erfüllt |
| Last | 1000 Spieler, 50 Befehle/s, 30 min, Teil mit tagelang nicht fortgeschriebenen Planeten | Lauf am 2026-10-26 (unten) | erfüllt |
| Frischer Aufbau | Leere DB → Migrationen → Admin → Universum → registrieren → Erzmine Stufe 1 | Lasttest-Lauf auf leerer DB (Migrationen); `WebApplicationTests` (Registrieren, Betreten, Bauauftrag, Admin); `BuildingEventTests` (Stufe nach Bauzeit) | durch Tests erfüllt, Browser-Durchlauf offen |
| Generator | Gleicher Seed → gleiche Karte; Abbruch hinterlässt kein halbes Universum | `UniverseGeneratorTests.The_same_seed_…`; `UniverseAdminServiceTests.Invalid_requests_store_nothing`; Übung am 2026-10-26 (unten) | erfüllt |
| Erweiterbarkeit | Zwei Test-Handler laufen ohne Änderung an Engine, Handlern, Schema | `ExtensibilityTests` | erfüllt |

---

## Lasttest

Werkzeug: `src/Galaxxis.Tools.LoadTest` (Ablauf und Bedienung im README des Projekts). Neu für die Abnahme:
Die Engine veröffentlicht ihre Verarbeitungszeiten als Metriken (`EngineMetrics`, Meter `Galaxxis.Engine`). Das
Werkzeug liest sie mit, wie es später auch Application Insights tun kann.

**Aufbau:** eigene LocalDB-Datenbanken (`GalaxxisLoadTest`, beim Start per Migration angelegt), Release-Build,
Engine im selben Prozess wie die Spieler-Simulation (wie heute in der Web-Anwendung), Poll-Intervall 1 s.
Das Universum hat 1300 Planeten und läuft seit 3 Tagen. 1000 Spieler treten über `IPlayerService` bei.
200 Planeten (20 %) sind seit 3 Tagen nicht fortgeschrieben. Befehle gehen über `ICommandService` wie bei der
Web-Anwendung: 60 % Ausbau, 40 % Produktionsgeschwindigkeit. Jeder Befehl wird wie im Browser einmal pro Sekunde
abgefragt, bis die Engine entschieden hat.

**Ergebnis: abgenommen.**

| Kriterium | Gemessen |
| --- | --- |
| Rückstand bleibt nahe 0 | Ältester fälliger Event: Median 0 s, 95 % 1 s, Maximum 1 s. Fällige Events: Median 6, Maximum 50 (= Befehle einer Sekunde bis zum nächsten Aufwachen). Aufholen nach dem letzten Befehl: 6 s |
| Verarbeitungszeit pro Event einstellig | Median **1,4 ms**, 95 % 2,4 ms, 99 % 3,6 ms, Maximum 97,5 ms (47 871 Events) |
| Keine doppelten Stufen | 5406 abgeschlossene Ausbauten auf 1000 Planeten, jede Stufe = Startstufe + abgeschlossene Ausbauten |
| Keine negativen Bestände | 1000 Planeten, gespeichert und bis jetzt fortgeschrieben |
| Sonstiges | 0 `Failed`, 0 unentschiedene Befehle, höchstens ein geplanter Auftrag pro Planet und Warteschlange, 0 Fehler bei 89 995 Befehlen |

Einordnung:

- **Durchsatzreserve.** Die Engine verarbeitete im Schnitt 26,5 Events/s und war dabei etwa 4 % der Zeit
  beschäftigt (1,6 ms × 26,5). Bei 1,6 ms pro Event liegt die Grenze eines Universums bei etwa 600 Events/s,
  also gut dem Zwanzigfachen der Last von 1000 Spielern.
- **Aufnahme eines Befehls** (Web-Anfrage ohne HTTP): Median 2,8 ms, 95 % 8,6 ms.
- **Entscheidung aus Spielersicht:** gut 1 s. Das ist ganz die Abfrage im Sekundentakt, die eigentliche Entscheidung
  fällt mit dem nächsten Aufwachen der Engine.
- 47 530 Ausbauanfragen wurden sofort abgelehnt, weil auf dem Planeten schon gebaut wurde. Das ist die Vorprüfung
  im `CommandService` und gewollt. Sechsmal kamen zwei Anfragen gleichzeitig durch die Vorprüfung; die Engine hat
  die zweite jeweils korrekt abgelehnt (Doppelklick-Schutz unter echter Last).
- Grenzen: LocalDB auf einem Entwicklungsrechner (12 logische Prozessoren), Engine und Simulation im selben Prozess,
  kein HTTP. Azure SQL hat höhere Latenzen pro Abfrage; die Reserve ist groß genug, das bleibt aber beim
  Azure-Deployment erneut zu messen (`TODO.md` Punkt 5).

<details>
<summary>Vollständiger Bericht des Werkzeugs</summary>

### Galaxxis load test 2026-10-26 13:13

**Result: ACCEPTED** (universe 3)

#### Acceptance criteria

| Criterion | Measured | Met |
| --- | --- | --- |
| Backlog stays near 0: oldest due event at most 3 s old in 95 % of the samples | 95 %: 1 s, max: 1 s | yes |
| Processing time per event in the single-digit milliseconds (median and 95 % below 10 ms) | median 1.4 ms, 95 %: 2.4 ms | yes |
| All correctness checks pass (no doubled levels, no negative stock, ...) | 5 of 5 passed | yes |
| No errors while giving commands | 0 error(s) | yes |

#### Settings and environment

| Setting | Value |
| --- | --- |
| Players | 1000 |
| Commands per second | 50 |
| Duration (planned / actual) | 00:30:00 / 00:30:00 |
| Stale planets | 20 %, 3 day(s) behind |
| Tick length | 00:00:01 |
| Machine | Microsoft Windows 10.0.26200, 12 logical processors |
| .NET | .NET 10.0.14 |
| Database server | (localdb)\MSSQLLocalDB |
| Engine | in this process, poll interval 00:00:01, at most 1000 events per universe and cycle |
| Build | Release |

#### Commands (players' view)

| Outcome | Count |
| --- | ---: |
| engine: Done | 36137 |
| engine: Rejected | 6 |
| engine: Scheduled | 6322 |
| recorded | 42465 |
| refused immediately: Another order of this kind is already in progress on this planet. | 47530 |
| sent | 89995 |

#### Timings (milliseconds)

| | Count | Average | Median | 95 % | 99 % | Max |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Record a command (web request) | 89995 | 3.6 | 2.8 | 8.6 | 10.7 | 178.2 |
| Command until decision seen (1 s status polling) | 42465 | 1010.5 | 1010.4 | 1019.1 | 1023.9 | 1179.2 |
| Engine: one event | 47871 | 1.6 | 1.4 | 2.4 | 3.6 | 97.5 |
| Engine: one cycle | 1812 | 42.5 | 41.8 | 61.0 | 78.8 | 298.5 |

#### Backlog (sampled every second)

| | Median | 95 % | Max |
| --- | ---: | ---: | ---: |
| Due events | 6 | 21 | 50 |
| Lag of the oldest due event (s) | 0 | 1 | 1 |

Samples: 1806. Engine throughput: 26.5 events per second on average. Catch-up after the last command: 6.0 s.

#### Events in the database at the end

| Type | State | Count |
| --- | --- | ---: |
| BuildBuilding | Scheduled | 916 |
| BuildBuilding | Done | 5406 |
| BuildBuilding | Rejected | 6 |
| ChangeProductionSpeed | Done | 36137 |

#### Correctness checks

| Check | Looked at | Passed |
| --- | --- | --- |
| No building upgraded twice (level = start + completed constructions) | 5406 completed constructions on 1000 planets | yes |
| No negative stock (stored and settled to now) | 1000 planets | yes |
| At most one scheduled order per planet and queue | 916 scheduled | yes |
| No failed events | 42465 events in total | yes |
| No command left undecided (requested more than 5 ticks ago) | 0 still requested | yes |

</details>

---

## Ausfallübung „harter Neustart“

Ablauf wie im README von `Galaxxis.Tools.LoadTest` beschrieben, Universum 4 (300 Spieler, 30 Befehle/s):

1. `run --players 300 --rate 30 --duration 00:05:00` gestartet, nach 1:29 min mit
   `Stop-Process -Force` hart beendet (kein Herunterfahren, keine Freigabe der Sperre).
2. Stand in der Datenbank direkt danach: 1131 `Done`, **7 `Requested`** (Befehle ohne Entscheidung),
   **300 `Scheduled`** (laufende Ausbauten, die meisten während der Ausfallzeit fällig). Universum nicht pausiert.
3. 3 Minuten Ausfallzeit, dann `verify 4` in einem neuen Prozess.

| Erwartet | Beobachtet |
| --- | --- |
| Kein Befehl fehlt | Alle 7 offenen Befehle und alle 300 Ausbauten verarbeitet: 1438 Events `Done`, 0 `Requested`, 0 `Failed` |
| Universum läuft ohne Admin-Klick weiter | Die neue Engine-Instanz meldet sofort „now processes universe 4“ (Sperre der beendeten Instanz war mit ihrer Verbindung freigegeben). 307 fällige Events, der älteste 201 s alt, nach **2,0 s** abgearbeitet |
| Ausfallzeit mit Produktion abgerechnet | Planet 1901: fortgeschrieben bis Tick 259 264 vor dem Absturz, danach bis Tick 259 367. Das ist der Tick seines während des Ausfalls fälligen Ausbaus, nicht der Zeitpunkt der Verarbeitung. Die Produktion bis dahin ist gutgeschrieben, die neue Stufe zählt ab ihrem eigenen Tick |
| Konsistenz | Alle Prüfungen von `verify` bestanden (300 abgeschlossene Ausbauten ohne doppelte Stufe, keine negativen Bestände) |

---

## Übung „Generator“

`UniverseGenerator generate --seed 42` zweimal hintereinander (Universen 5 und 6, Lasttest-Datenbank): Beide
heißen „Solariion“ und haben 10 Galaxien mit 1000 Planeten. Ein Vergleich per SQL über alle Planeten in Id-Reihenfolge
(Planetenname, Koordinaten, Name und Koordinaten des Sternsystems) ergibt **1000 von 1000 identisch**. Dass ein
Abbruch nichts Halbes hinterlässt, sichert ein einziger `SaveChanges`-Aufruf (eine Transaktion) in
`UniverseAdminService.GenerateAsync`; `UniverseAdminServiceTests.Invalid_requests_store_nothing` prüft, dass
ungültige Anfragen nichts speichern.

---

## Offen

- **Frischer Aufbau im Browser** einmal von Hand durchspielen (leere Datenbank → `bootstrap-admin` → `generate
  --seed 42` → `resume` → Admin-Login → registrieren → Universum betreten → Erzmine bauen → nach der Bauzeit
  Stufe 1). Alle Einzelschritte sind durch Tests abgedeckt, der Durchlauf von Hand als Ganzes noch nicht.
- Lasttest auf Azure wiederholen, sobald das Deployment steht.
