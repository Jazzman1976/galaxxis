# Galaxxis — TODO

Stand: 2026-09-16

Das aktuelle Ziel ist das **Refactoring der Basisarchitektur** (Schritte 1–6, siehe `CLAUDE.md`).
Die folgenden Punkte kommen **danach**, in dieser Reihenfolge.

## 1. Zeitnah: fehlende Gebäude-Oberflächen

Diese Gebäude und Befehle waren bisher nur über die alte Home-Seite bedienbar. Die Home-Seite fällt in
Schritt 5 weg; die Engine-Handler bleiben erhalten, es fehlt nur die Oberfläche.

- [ ] MageAcademy
- [ ] ElementiumSynthesizer
- [ ] Shipyard (Schiffsproduktion)
- [ ] DefenseSystem (Verteidigungsproduktion)
- [ ] Flottenbefehle: Angriff, Scan, Bewegung

Umsetzung nach dem Muster der Erzmine (Application-Dienst + Controller + AJAX-Statusanzeige).

## 2. Handel zwischen Spielern (zentrales Feature)

- [ ] Konzept ausarbeiten: Angebotsarten, Gebühren, Transportweg, Punkte laut Konzept §2
- [ ] Umsetzung als eigener Feature-Ordner `Trading/` in allen Modulen
  (Reservieren beim Annehmen, Übertragen beim Abschluss, Erstatten bei Abbruch)

## 3. Quests im Universum

- [ ] Konzept ausarbeiten: Quest-Arten, Belohnungen, Spawn-Regeln
- [ ] Umsetzung als Feature-Ordner `Quests/`; das Erscheinen läuft über wiederkehrende System-Events

## 4. Kampfsystem

- [ ] Umsetzung laut `docs/KonzeptWord.fodt` §7 (Treffgenauigkeit, Schilde, Initiative, Kapern, Trümmer)

## 5. Azure-Deployment

- [ ] App Service (Web), Azure SQL (beide Datenbanken)
- [ ] Engine als eigener Dienst (Container App oder WebJob)
- [ ] Key Vault für Geheimnisse, Application Insights für Logging
- [ ] Data-Protection-Keys im Blob Storage (nötig bei mehreren Web-Instanzen)

## 6. Admin-Oberfläche für Universen

- [ ] Universen im Web generieren, prüfen und freigeben (nutzt `IUniverseGenerationService`)
