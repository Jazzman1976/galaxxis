# Galaxxis — TODO

Stand: 2026-09-17

Das aktuelle Ziel ist das **Refactoring der Basisarchitektur** (Schritte 1–6, siehe `CLAUDE.md`).
Die folgenden Punkte kommen **danach**, in dieser Reihenfolge.

## 1. Zeitnah: fehlende Oberflächen

Die alte Home-Seite ist in Schritt 5 entfallen. Alle sieben Ressourcengebäude (auch MageAcademy und
ElementiumSynthesizer) haben jetzt eine gemeinsame Gebäudeseite (`Views/Building`): Ausbau und
Produktionsgeschwindigkeit. Für die folgenden Befehle existieren die Engine-Handler, es fehlen Application-Dienst
und Oberfläche:

- [x] MageAcademy, ElementiumSynthesizer (über die gemeinsame Gebäudeseite)
- [ ] Shipyard: Ausbau und Schiffsproduktion
- [ ] DefenseSystem: Ausbau und Verteidigungsproduktion
- [ ] Flottenbefehle: Angriff, Scan, Bewegung

Umsetzung nach dem Muster der Gebäudeseite: Methode in `ICommandService`, Controller in `Views/<Feature>`,
Formular mit `data-command-form` (Statusabfrage über `commands.js`).

Kleinere Punkte der Oberfläche:

- [ ] Ablehnungsgründe der Engine (`GameEventMessages`) sind noch englisch; für Spieler übersetzen
- [ ] Liste der eigenen Planeten pro Universum (heute nur über die Karte mit * erreichbar)
- [ ] Front-End-Build modernisieren: `node-sass` und `gulp 4` sind veraltet (z. B. auf `sass` umstellen)

## 2. Handel zwischen Spielern (zentrales Feature)

- [ ] Konzept ausarbeiten: Angebotsarten, Gebühren, Transportweg, Punkte laut Konzept §2
- [ ] Umsetzung als eigener Feature-Ordner `Trading/` in allen Modulen
  (Reservieren beim Annehmen, Übertragen beim Abschluss, Erstatten bei Abbruch)

## 3. Quests im Universum

- [ ] Konzept ausarbeiten: Quest-Arten, Belohnungen, Spawn-Regeln
- [ ] Umsetzung als Feature-Ordner `Quests/`; das Erscheinen läuft über wiederkehrende System-Events

## 4. Kampfsystem

- [ ] Umsetzung laut `docs/concept/KonzeptWord.fodt` §7 (Treffgenauigkeit, Schilde, Initiative, Kapern, Trümmer)

## 5. Azure-Deployment

- [ ] App Service (Web), Azure SQL (beide Datenbanken)
- [ ] Engine als eigener Dienst (Container App oder WebJob)
- [ ] Key Vault für Geheimnisse, Application Insights für Logging
- [ ] Data-Protection-Keys im Blob Storage (nötig bei mehreren Web-Instanzen)

## 6. Admin-Oberfläche für Universen

- [x] Universen im Web generieren, starten und pausieren (`/admin`, nutzt `IUniverseAdminService`)
- [ ] Ein pausiertes Universum vor der Freigabe ansehen (Karte ohne Beitritt)
