# Galaxxis — Dokumentation

Hier liegt alles, was gemeinsam entschieden wurde und für Mitarbeitende gedacht ist. Technische Details zu den
einzelnen Modulen stehen in den `README.md` der Projekte unter `src/`, die Arbeitsregeln für den Code in der
`CLAUDE.md` im Repository-Root.

| Ordner | Inhalt |
| --- | --- |
| [`concept/`](concept/) | Spielkonzept (Deutsch, OpenDocument): `ConzeptExcel.fods` ist die verbindliche Quelle aller Zahlen, `KonzeptWord.fodt` beschreibt Spielablauf, Rassen und Kampf. In `archive/` liegen überholte Fassungen. |
| [`guides/`](guides/) | Anleitungen zum Einrichten und Betreiben: [`INSTALLATION.md`](guides/INSTALLATION.md) (vom leeren Rechner bis zum laufenden Spiel, Server-Betrieb, häufige Probleme). |
| [`plans/`](plans/) | Freigegebene Pläne und die Aufgabenliste: [`FOUNDATION-REFACTORING-PLAN.md`](plans/FOUNDATION-REFACTORING-PLAN.md) (Umbau der Basisarchitektur), [`TODO.md`](plans/TODO.md) (was danach kommt). |
| [`archive/`](archive/) | Abgelöstes, das als Referenz erhalten bleibt: [`dummy-prototype/`](archive/dummy-prototype/) ist der statische HTML/Vue-Oberflächenentwurf von 2020 (im Browser `index.html` öffnen). Nicht Teil des Builds. |
| [`audits/`](audits/) | Prüfberichte und ihre Befunde: [`AUDIT-2026-09-16.md`](audits/AUDIT-2026-09-16.md) (Anlass für den Umbau, mit Erledigt-Status), [`ACCEPTANCE-2026-10-26.md`](audits/ACCEPTANCE-2026-10-26.md) (Abnahme des Umbaus: Lasttest und Ausfallübungen). |

## Regeln

- Neue Dokumente kommen in den passenden Unterordner. Eine neue Art von Dokument (zum Beispiel Entscheidungen
  oder Spielregeln für Handel und Quests) bekommt einen eigenen Unterordner und eine Zeile in dieser Tabelle.
- Nur lokale Arbeitsnotizen, Entwürfe und Werkzeugdateien gehören nicht hierher, sondern in `.ignore/` im
  Repository-Root. Der Ordner ist über die globalen Git-Ausnahmen ignoriert und wird nie eingecheckt.
- Die `.fods`/`.fodt`-Dateien sind flaches XML. Die Sperrdateien, die LibreOffice beim Öffnen anlegt, sind über
  `docs/.gitignore` ausgeschlossen.
