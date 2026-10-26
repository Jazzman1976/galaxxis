# Galaxxis - Ein Fantasy Online Browser Game

V1.1 - 22 März 2015

> **Hinweis zu dieser Datei:** Markdown-Übertragung von [`KonzeptWord.fodt`](KonzeptWord.fodt) (V1.1, 2015). Der Text
> des Originals ist unverändert übernommen, auch Schreibweisen und Zahlen. Alles, was nicht im Original steht
> (Diagramme, Legenden, Querverweise, Anmerkungen), ist als **Ergänzung** in solchen Hinweisblöcken markiert.
>
> Die Zahlen der Gebäude in Kapitel 8 sind älter als die Tabelle [`ConzeptExcel.fods`](ConzeptExcel.fods). Wo beide
> sich widersprechen, gilt die Tabelle; nach ihr richten sich auch die Regelklassen in `src/Galaxxis.Domain`.

## Inhalt

- [1 Einführung](#1-einführung)
  - [1.1 Die wichtigsten Merkmale des Spiels](#11-die-wichtigsten-merkmale-des-spiels)
- [2 Ziel des Spiels (Punkte und Level)](#2-ziel-des-spiels-punkte-und-level)
- [3 Ressourcen](#3-ressourcen)
- [4 Rassen allgemein](#4-rassen-allgemein)
  - [4.1 Rohstoff Spezialisierung](#41-rohstoff-spezialisierung)
  - [4.2 Waffen und Rüstung](#42-waffen-und-rüstung)
  - [4.3 Strategie](#43-strategie)
- [5 Rassen im Detail](#5-rassen-im-detail)
  - [5.1 Menschen](#51-menschen)
  - [5.2 Zwerge](#52-zwerge)
  - [5.3 Elfen](#53-elfen)
  - [5.4 Goblins](#54-goblins)
  - [5.5 Trolle](#55-trolle)
  - [5.6 Orks](#56-orks)
- [6 Wachstumsraten](#6-wachstumsraten)
  - [6.1 Wachstumsfaktor Rohstoffe](#61-wachstumsfaktor-rohstoffe)
  - [6.2 Wachstumsfaktor Versorgung](#62-wachstumsfaktor-versorgung)
  - [6.3 Wachstumsfaktor Forschung](#63-wachstumsfaktor-forschung)
  - [6.4 Wachstumsfaktor Konstruktion](#64-wachstumsfaktor-konstruktion)
- [7 Kampfsystem](#7-kampfsystem)
  - [7.1 Gemeinsame Kampf Attribute](#71-gemeinsame-kampf-attribute)
  - [7.2 Zusätzliche Schiffs Attribute](#72-zusätzliche-schiffs-attribute)
  - [7.3 Basis Kampfablauf - Beispiel](#73-basis-kampfablauf---beispiel)
- [8 Gebäude](#8-gebäude)
  - [8.1 Rohstoff Gebäude](#81-rohstoff-gebäude)

## 1 Einführung

In Galaxxis spielst du einen Space Commander einer speziellen Fantasy Rasse. Du kommandierst mehrere Planeten, Schiffe
und Flotten. Du musst die notwendige Infrastruktur auf mehreren Planeten aufbauen und rassenspezifische Güter
herstellen, die zur Herstellung von Schiffen und Verteidigungsanlagen benötigt werden. Jede Rasse hat spezielle Vor-
und Nachteile. Du kannst deine hergestellten Güter, Schiffe und Verteidigungsanlagen mit anderen Spielern handeln oder
diese in Kämpfen erbeuten.

### 1.1 Die wichtigsten Merkmale des Spiels

- Handel mit anderen Spielern oder Quests erledigen soll eine gleichwertige Alternative zu Kämpfen sein um Punkte zu
  erhalten.
- Rassen spezifische Vor- und Nachteile sollen dazu beitragen strategische Entscheidungen zu treffen.
- Du kannst Quests erledigen um Punkte zu bekommen und um Güter, Schiffe und Verteidigungsanlagen anderer Rassen zu
  erbeuten.
- Deine Schiffe und Verteidigungsanlagen haben Sockel die mit bestimmten Gütern ausgerüstet werden können um sie zu
  verstärken oder ihnen bestimmte rassenspezifische Eigenschaften zu geben. Dadurch gibt es vielseitige strategische
  Möglichkeiten Flotten oder Verteidigungsanlagen zusammen zu stellen.

## 2 Ziel des Spiels (Punkte und Level)

Für jedes Gebäude das gebaut oder jede Forschung die geforscht wird erhält man Punkte entsprechend des Levels^2 des
gebauten oder geforschten Objektes. Wenn ein Gebäude abgerissen wird gehen die Punkte verloren. Forschungen sind
permanent und die Punkte daher auch.

Händler bekommen permanente Punkte für jeden Handel mit anderen Spielern. Die Punkte sind abhängig von der Art des
gehandelten Objektes. Für Güter gilt Level des Guts = Punkte. Für Schiffe und Verteidigungsanlagen gilt
Strukturpunkte / 10 = Punkte.

Angreifer erhalten permanente Punkte für jedes Schiff und jede Verteidigungsanlage die von einer eigenen Flotte
zerstört oder erobert werden. Dabei gilt Strukturpunkte / 10 = Punkte. Damit schwächere Spieler geschützt sind bekommt
man keine Punkte mehr wenn der angegriffene Spieler 10 Level niedriger als der Angreifer ist. Spieler die weniger als
die Hälfte des Levels des Angreifers haben (abgerundet) können gar nicht angegriffen werden.

Man erhält permanente Punkte für das Erledigen von Quests. Wie viele Punkte man für Quests bekommt ist von der Quest
Schwierigkeit abhängig.

Weiterhin erhält man permanente Punkte durch das Erreichen von Erfolgen, z.B. „10 Kämpfe gewonnen“ oder „10 Quests
erledigt“.

Abhängig von den Punkten erreicht man einen höheren Level. Pro Level kann man 10% mehr Schiffe kommandieren
(abgerundet). Man startet auf Level 1 mit 100 Schiffen. Die Level Grenzen errechnen sich über die Formel neue Level
Punktegrenze = aktuelle Level Punktegrenze + (Level – 1) * 1000.

| Level | Punkte | Schiffe |
| --- | --- | --- |
| 1 | 0 | 100 |
| 2 | 1000 = 0 + (2-1)*1000 | 110 = 100 * 1,1 |
| 3 | 3000 = 1000 + (3-1)*1000 | 121 = 110 * 1,1 |
| 4 | 6000 = 3000 + (4-1)*1000 | 133 = 121 * 1,1 |
| 5 | 10000 = 6000 + (5-1)*1000 | 146 = 133 * 1,1 |
| 6 | 15000 = 10000 + (6-1)*1000 | 160 = 146 * 1,1 |
| usw.… | usw.… | usw.… |

Eine Top Spieler Liste zeigt wer aktuell der Herrscher des Universums ist.

> **Ergänzung:** Übersicht der Punktequellen. Nur Gebäudepunkte können wieder verloren gehen.
>
> ```mermaid
> flowchart LR
>     Gebaeude["Gebäude bauen<br>Level²"] -->|"verloren bei Abriss"| Punkte
>     Forschung["Forschung<br>Level²"] -->|permanent| Punkte
>     Handel["Handel<br>Gut: Level<br>Schiff/Anlage: SP / 10"] -->|permanent| Punkte
>     Kampf["Kampf: zerstört/erobert<br>SP / 10"] -->|permanent| Punkte
>     Quests["Quests<br>nach Schwierigkeit"] -->|permanent| Punkte
>     Erfolge["Erfolge"] -->|permanent| Punkte
>     Punkte --> Level["Level<br>+10 % Schiffe je Level"]
> ```
>
> Geschlossene Form der Levelgrenze: Punktegrenze(L) = 500 · L · (L − 1), Schiffe(L) = 100 · 1,1^(L−1), jeweils
> abgerundet. Ob in der Schiffstabelle pro Schritt (wie im Original) oder erst am Ende abgerundet wird, ist offen.

## 3 Ressourcen

In Galaxxis werden sieben verschiedene Ressourcen verwendet. Sie sind in drei Kategorien eingeteilt und werden für drei
verschiedene Anwendungsarten benutzt.

Die drei Kategorieen sind:

- Basis Rohstoffe
- Spezielle Rohstoffe
- Elementium

Die drei Anwendungsarten sind:

- Konstruktion
- Versorgung
- Güterproduktion

| Kategorie I => Basis Rohstoffe | Anwendungsart |
| --- | --- |
| Erz | Konstruktion |
| Energie | Versorgung |
| Werkzeuge | Güterproduktion |

| Kategorie II => Spezielle Rohstoffe | Anwendungsart |
| --- | --- |
| Mana | Konstruktion und Versorgung |
| Kristalle | Konstruktion und Güterproduktion |
| Antimaterie | Versorgung und Güterproduktion |

| Kategorie III => Elementium | Anwendungsart |
| --- | --- |
| Elementium | Konstruktion, Versorgung und Güterproduktion |

> **Ergänzung:** Dieselbe Zuordnung als Diagramm. Jede Anwendungsart wird von genau einem Basis Rohstoff, zwei
> speziellen Rohstoffen und Elementium bedient.
>
> ```mermaid
> flowchart LR
>     subgraph K1["Kategorie I – Basis Rohstoffe"]
>         Erz
>         Energie
>         Werkzeuge
>     end
>     subgraph K2["Kategorie II – Spezielle Rohstoffe"]
>         Mana
>         Kristalle
>         Antimaterie
>     end
>     subgraph K3["Kategorie III"]
>         Elementium
>     end
>     Erz --> Konstruktion
>     Mana --> Konstruktion
>     Kristalle --> Konstruktion
>     Elementium --> Konstruktion
>     Energie --> Versorgung
>     Mana --> Versorgung
>     Antimaterie --> Versorgung
>     Elementium --> Versorgung
>     Werkzeuge --> Gueter["Güterproduktion"]
>     Kristalle --> Gueter
>     Antimaterie --> Gueter
>     Elementium --> Gueter
> ```

## 4 Rassen allgemein

Man kann zwischen sechs verschiedenen Rassen auswählen. Jede Rasse hat bestimmte Vor- und Nachteile.

### 4.1 Rohstoff Spezialisierung

Jede Rasse spezialisiert sich auf zwei Arten von Ressourcen (eine primär, die andere sekundär). Abhängig von dieser
Spezialisierung kann jede Rasse bestimmte Rohstoffe besser oder weniger gut produzieren. Die Berechnung folgt stets
diesem Schlüssel:

Schema für die Produktion von Rohstoffen:

- Primäre Rohstoffart Spezialisierung in Kategorie I => 1.5x
- Primäre + sekundäre Rohstoffart Spezialisierung in Kategorie II => 1.5x
- Fehlende primäre Spezialisierung (nur sekundäre) in Kategorie II => 0.75x

> **Ergänzung:** Mit „Rohstoffart“ ist die Anwendungsart aus Kapitel 3 gemeint. Beispiel Menschen (primär Versorgung,
> sekundär Güterproduktion): Kategorie I der Versorgung ist Energie → x 1,5. In Kategorie II deckt Antimaterie beide
> Spezialisierungen ab → x 1,5; Kristalle decken nur die sekundäre (Güterproduktion) ab → x 0,75. Das ergibt genau die
> Werte in Kapitel 5.

### 4.2 Waffen und Rüstung

Eine Rasse verwendet eine von drei möglichen Kombinationen aus Waffen- und Rüstungstechnologie. Jede Waffentechnologie
ist besonders stark gegen eine, und etwas schwächer gegen eine andere Rüstungstechnologie.

Stark bedeutet, man verursacht 1,5-fachen Schaden.

Schwach bedeutet man verursacht nur den 0,75-fachen Schaden.

| Kombination | Beschreibung |
| --- | --- |
| Kinetische Waffen und strukturelle Rüstungen | Projektilwaffen werden abgefeuert und die Strukturen werden mit Stahlpanzerplatten gegen eingehenden Schaden geschützt.<br><br>Kinetische Waffen sind stark gegen antimagische Rüstungen und schwach gegen strukturelle Rüstungen. |
| Magische Waffen und Antimagische Rüstungen | Magische Kräfte werden eingesetzt um Schaden zu verursachen und antimagische Schutzzauber veringern eingehenden Schaden.<br><br>Magische Waffen sind stark gegen kompensierende Rüstungen und schwach gegen antimagische Rüstungen. |
| Energetische Waffen und kompensierende Rüstungen | Gebündelte Energie wird eingesetzt um hohen energetischen Schaden zu verursachen und die Struktur wird durch Energie kompensierende Mechanismen gegen eingehenden Schaden geschützt.<br><br>Energetische Waffen sind stark gegen strukturelle Rüstungen und schwach gegen kompensierende Rüstungen. |

> **Ergänzung:** Die Beziehungen bilden ein Dreieck (Schere-Stein-Papier). Jede Waffe ist schwach gegen die Rüstung
> der eigenen Kombination; gegen die dritte Rüstung macht sie normalen Schaden (100 %).
>
> ```mermaid
> flowchart LR
>     KW["Kinetische Waffen"] -->|"stark x 1,5"| AR["Antimagische Rüstung"]
>     MW["Magische Waffen"] -->|"stark x 1,5"| KR["Kompensierende Rüstung"]
>     EW["Energetische Waffen"] -->|"stark x 1,5"| SR["Strukturelle Rüstung"]
>     KW -.->|"schwach x 0,75"| SR
>     MW -.->|"schwach x 0,75"| AR
>     EW -.->|"schwach x 0,75"| KR
> ```
>
> | Waffe ↓ / Rüstung → | Strukturell | Antimagisch | Kompensierend |
> | --- | --- | --- | --- |
> | Kinetisch | 0,75 | **1,5** | 1,0 |
> | Magisch | 1,0 | 0,75 | **1,5** |
> | Energetisch | **1,5** | 1,0 | 0,75 |

### 4.3 Strategie

Eine Rasse verfolgt entweder eine offensive oder eine defensive Strategie. Je nach Strategie erhält man einen Bonus auf
offensive oder defensive Fähigkeiten.

## 5 Rassen im Detail

Die folgenden Rassen sind in Galaxxis spielbar.

### 5.1 Menschen

| | | | |
| --- | --- | --- | --- |
| Primäre Spezialisierung | Versorgung | Sekundäre Spezialisierung | Güterproduktion |
| Bonus | Energie x 1,5<br>Antimaterie x 1,5 | Malus | Kristalle x 0,75 |
| Waffentechnologie | Kinetische Waffen | Rüstungstechnologie | Strukturelle Rüstung |
| Strategie | Defensiv | | |

### 5.2 Zwerge

| | | | |
| --- | --- | --- | --- |
| Primäre Spezialisierung | Konstruktion | Sekundäre Spezialisierung | Güterproduktion |
| Bonus | Erz x 1,5<br>Kristalle x 1,5 | Malus | Antimaterie x 0,75 |
| Waffentechnologie | Kinetische Waffen | Rüstungstechnologie | Strukturelle Rüstung |
| Strategie | Offensiv | | |

### 5.3 Elfen

| | | | |
| --- | --- | --- | --- |
| Primäre Spezialisierung | Versorgung | Sekundäre Spezialisierung | Konstruktion |
| Bonus | Energie x 1,5<br>Mana x 1,5 | Malus | Kristalle x 0,75 |
| Waffentechnologie | Magische Waffen | Rüstungstechnologie | Antimagische Rüstung |
| Strategie | Defensiv | | |

### 5.4 Goblins

| | | | |
| --- | --- | --- | --- |
| Primäre Spezialisierung | Konstruktion | Sekundäre Spezialisierung | Versorgung |
| Bonus | Erz x 1,5<br>Mana x 1,5 | Malus | Antimaterie x 0,75 |
| Waffentechnologie | Magische Waffen | Rüstungstechnologie | Antimagische Rüstung |
| Strategie | Offensiv | | |

### 5.5 Trolle

| | | | |
| --- | --- | --- | --- |
| Primäre Spezialisierung | Güterproduktion | Sekundäre Spezialisierung | Versorgung |
| Bonus | Werkzeuge x 1,5<br>Antimaterie x 1,5 | Malus | Mana x 0,75 |
| Waffentechnologie | Energetische Waffen | Rüstungstechnologie | Kompensierende Rüstung |
| Strategie | Defensiv | | |

### 5.6 Orks

| | | | |
| --- | --- | --- | --- |
| Primäre Spezialisierung | Güterproduktion | Sekundäre Spezialisierung | Konstruktion |
| Bonus | Werkzeuge x 1,5<br>Kristalle x 1,5 | Malus | Mana x 0,75 |
| Waffentechnologie | Energetische Waffen | Rüstungstechnologie | Kompensierende Rüstung |
| Strategie | Offensiv | | |

> **Ergänzung:** Alle sechs Rassen auf einen Blick. Jede der sechs Kombinationen aus primärer und sekundärer
> Spezialisierung kommt genau einmal vor; je Waffen-/Rüstungstechnologie gibt es eine offensive und eine defensive
> Rasse.
>
> | Rasse | Primär | Sekundär | Bonus x 1,5 | Malus x 0,75 | Waffen / Rüstung | Strategie |
> | --- | --- | --- | --- | --- | --- | --- |
> | Menschen | Versorgung | Güterproduktion | Energie, Antimaterie | Kristalle | Kinetisch / Strukturell | Defensiv |
> | Zwerge | Konstruktion | Güterproduktion | Erz, Kristalle | Antimaterie | Kinetisch / Strukturell | Offensiv |
> | Elfen | Versorgung | Konstruktion | Energie, Mana | Kristalle | Magisch / Antimagisch | Defensiv |
> | Goblins | Konstruktion | Versorgung | Erz, Mana | Antimaterie | Magisch / Antimagisch | Offensiv |
> | Trolle | Güterproduktion | Versorgung | Werkzeuge, Antimaterie | Mana | Energetisch / Kompensierend | Defensiv |
> | Orks | Güterproduktion | Konstruktion | Werkzeuge, Kristalle | Mana | Energetisch / Kompensierend | Offensiv |

## 6 Wachstumsraten

Es werden verschiedene Wachstumsraten verwendet. Die grundlegende Berechnung einer Wachstumsrate ist:

```
[BASISWERT] * ( [WACHSTUMSFAKTOR] * ( [LEVEL] – 1 ) ^ 2 + 1 )
```

Berechnet werden immer die Kosten die benötigt werden um ein Objekt auf das [LEVEL + 1] auszubauen, Technologieen von
[LEVEL + 1] zu erforschen, um Objekte dieses [LEVELS] herzustellen oder um ein Gebäude auf diesem [LEVEL] zu betreiben.

Für die Basiswerte gelten folgende Richtwerte:

| Richtwert | Basiswert |
| --- | --- |
| Sehr wenig | 5 |
| Wenig | 10 |
| Durchschnittlich | 15 |
| Hoch | 20 |
| Sehr hoch | 25 |

Die Basiswerte für Rohstoffe weichen von diesem Muster ab, da sie auf 24 Stunden ausgelegt sind. Außerdem wird auf die
Rohstoffe noch ein individueller Grundertrag hinzu addiert.

> **Ergänzung:** Umgesetzt in `src/Galaxxis.Domain/Growth/GrowthRate.ValueAt`. Die Tabelle `ConzeptExcel.fods` kennt
> zusätzlich den Wachstumsfaktor 5 für die Bauzeit. Beispiel Basiswert 10:
>
> | Level | Faktor 1 | Faktor 2 | Faktor 3 | Faktor 4 |
> | --- | --- | --- | --- | --- |
> | 1 | 10 | 10 | 10 | 10 |
> | 2 | 20 | 30 | 40 | 50 |
> | 3 | 50 | 90 | 130 | 170 |
> | 5 | 170 | 330 | 490 | 650 |
> | 10 | 820 | 1630 | 2440 | 3250 |

### 6.1 Wachstumsfaktor Rohstoffe

Für Rohstoffe wird der Wachstumsfaktor 1 verwendet.

### 6.2 Wachstumsfaktor Versorgung

Für die Versorgung wird der Wachstumsfaktor 2 verwendet.

### 6.3 Wachstumsfaktor Forschung

Für das Erforschen einer Technologie auf die nächst höhere Stufe wird der Wachstumsfaktor 3 verwendet.

### 6.4 Wachstumsfaktor Konstruktion

Für das Ausbauen eines Gebäudes auf die nächst höhere Stufe wird der Wachstumsfaktor 4 verwendet.

## 7 Kampfsystem

Gekämpft wird in Galaxxis mit Schiffen und Verteidigungsanlagen.

### 7.1 Gemeinsame Kampf Attribute

Gemeinsame Kampf Attribute sind Attribute die bei Schiffen und auch bei Verteidigungsanlagen zu finden sind.

#### 7.1.1 Treffgenauigkeit

Die Treffgenauigkeit ist die Wahrscheinlichkeit ein Ziel zu treffen. Sie wird in % angegeben und bei einem Angriff
entsprechend ausgewürfelt. Ist das Würfelergebnis <= der Treffgenauigkeit hat man sein Ziel getroffen. Andernfalls
trifft man das Ziel nicht. Hard-Cap der Treffgenauigkeit ist 95%. Die Treffgenauigkeit ist quasi der Attackewert. Es
ist ein offensives Attribut.

Scheitert ein Angriff an der Treffgenauigkeit erhöht sich diese nur bis getroffen wird um 10%. Danach ist die Chance
wieder die Ausgangschance. Dieser Bonus kann das Hard-Cap nicht überschreiten.

#### 7.1.2 Schildstärke

Die Schildstärke gibt an mit welcher Wahrscheinlichkeit ein Treffer von den Schilden des Schiffes komplett absorbiert
wird. Sie wird in % angegeben und bei einem Treffer entsprechend ausgewürfelt. Ist das Würfelergebnis <= der
Schildstärke halt das Schild und der Angriff wird komplett absorbiert. Andernfalls werden die Schilde überwunden und
man wird getroffen. Hard-Cap der Schildstärke ist 65%. Die Schildstärke ist quasi der Paradewert. Sie ist ein
defensives Attribut.

Verhindert die Schildstärke einen Treffer verringert sich diese um 10% nur solange bis ein Treffer nicht mehr
verhindert wird. Danach ist die Chance wieder die Ausgangschance.

> **Ergänzung:** Im Beispiel (7.3.5) sind die 10 % bei Treffgenauigkeit und Schildstärke Prozentpunkte
> (45 % → 55 %, 20 % → 10 %).

#### 7.1.3 Angriffsstärke

Die Angriffsstärke ist ein absoluter Punktewert der angibt wieviel Schaden ein Treffer am Ziel verursacht.

#### 7.1.4 Schiffspanzerung

Die Schiffspanzerung ist ein absoluter Punktewert der angibt, wieviel Punkte von der Angriffsstärke des Angreifers
abgezogen wird. Die Differenz aus Angriffsstärke-Schiffspanzerung gibt an wieviel Trefferpunkte ein Angriff verursacht.

```
Angriffsstärke – Schiffspanzerung = Trefferpunkte
```

Die Schiffspanzerung ist quasi der Rüstungswert.

#### 7.1.5 Waffengattung

Die Waffengattung gibt an welche Art von Schaden verursacht wird. Es gibt drei mögliche Waffengattungen und damit
Schadensarten:

- Kinetische Waffe (Projektilwaffen, Feuerwaffen, Kanonen, etc.)
- Energetische Waffe (Laser, Phaser, Ultraschall, etc.)
- Magische Waffen (Magische Entladungen verschiedener Ausrichtung)

Grundsätzlich nutzt ein Schiff oder eine Verteidigungsanlage die Waffengattung des Erbauers, macht also erstmal
grundsätzlich 100% Schaden mit der Schadensart der Waffengattung (kinetisch, energetisch oder magisch).

#### 7.1.6 Rüstungstyp

Analog zu den Waffengattungen gibt es genau drei mögliche Rüstungstypen die sich auf die Reduzierung von Schadensarten
spezialisiert haben:

- Strukturelle Rüstung (Stahlplatten, etc.)
- Kompensierende Rüstugn (Kondensatoren, Energieabsorber, Energieumwandler, etc.)
- Antimagie Rüstung (Magische Schutzzauber oder Barrieren, etc.)

Jeder Rüstungstyp ist stark gegen eine Waffengattung und schwach gegen eine andere Waffengattung. Daher gilt:

**Kinetische Waffen**
Stark gegen Antimagie Rüstung.
Schwach gegen Strukturelle Rüstung.

**Energetische Waffen**
Stark gegen Strukturelle Rüstung.
Schwach gegen Kompensierende Rüstung.

**Magische Waffen**
Stark gegen Kompensierende Rüstung.
Schwach gegen Antimagische Rüstung.

Stark bedeutet man macht den 1,5-fachen Schaden (150%).

Schwach bedeutet man macht nur ¾ Schaden (75%).

Trefferpunkte werden durch den Rüstungstyp modifiziert um die Schadenspunkte zu bestimmen. Davon werden noch die
Rüstungspunkte abgezogen.

```
( Trefferpunkte * Rüstungsmodifikator ) - Rüstungspunkte = Schadenspunkte.
```

Schadenspunkte werden von den Strukturpunkten des Ziels direkt abgezogen.

> **Ergänzung:** 7.1.4 und 7.1.6 ziehen die Panzerung beide ab; zusammen gelesen würde sie doppelt zählen. Das
> Beispiel in 7.3.5 zieht sie genau einmal ab, und zwar nach dem Modifikator:
> `Schadenspunkte = Angriffsstärke * Rüstungsmodifikator - Schiffspanzerung` (z. B. 40 · 1,5 − 10 = 50).

#### 7.1.7 Strukturpunkte

Schadenspunkte werden von den Strukturpunkten abgezogen. Sinken die Strukturpunkte auf <= 0, dann wird das Objekt
zerstört oder gekapert. Die Strukturpunkte sind also die Lebensenergie des Objektes.

#### 7.1.8 CPU Leistung

Die CPU Leistung wird für verschiedene Berechnungen verwendet und ist ein Leistungsindex des Objektes.

**Initiative**

Die CPU Leistung bestimmt wer zuerst einen Angriff ausführen darf (Initiative). Stehen sich zwei Gegner gegenüber wird
die CPU Leistung verglichen. Ist sie gleich wird zufällig bestimmt wird angreifen darf. Ansonsten greift derjenige
zuerst an, der die höhere CPU Leistung hat.

**Kapern**

Sobald die Strukturpunkte auf <= 0 sinken wird ein Objekt zerstört oder gekapert (erobert). Ob ein Objekt gekapert oder
zerstört wird wird durch die CPU Leistung des Angreifers und des zerstörten Objektes berechnet. Dabei gilt:

```
Kaperchance = CPU Leistung Angreiffer / ( 20 * CPU Leistung zerstörtes Objekt ) * 100%
```

Die Kaperchance wird dann ausgewürfelt. Ist das Würfelergebnis <= der Kaperchance, dann wird das Objekt gekapert,
ansonsten wird es zerstört.

> **Ergänzung:** Ablauf eines einzelnen Angriffs nach 7.1.1 bis 7.1.8.
>
> ```mermaid
> flowchart TD
>     A["Angriff"] --> TG{"Wurf <= Treffgenauigkeit?<br>(max. 95 %)"}
>     TG -- nein --> TGB["Verfehlt<br>Treffgenauigkeit +10 % bis zum nächsten Treffer"]
>     TG -- ja --> SS{"Wurf <= Schildstärke des Ziels?<br>(max. 65 %)"}
>     SS -- ja --> SSM["Schild hält, Treffer absorbiert<br>Schildstärke −10 % bis zum nächsten Durchschlag"]
>     SS -- nein --> DMG["Schadenspunkte =<br>Angriffsstärke × Rüstungsmodifikator − Panzerung"]
>     DMG --> SP["Strukturpunkte −= Schadenspunkte"]
>     SP --> SP0{"Strukturpunkte <= 0?"}
>     SP0 -- nein --> Ende["Ziel bleibt im Kampf"]
>     SP0 -- ja --> KC{"Wurf <= Kaperchance?<br>CPU Angreifer / (20 × CPU Ziel)"}
>     KC -- ja --> Gekapert["Gekapert"]
>     KC -- nein --> Zerstoert["Zerstört"]
> ```

### 7.2 Zusätzliche Schiffs Attribute

Folgende Attribute haben nur Schiffe. Sie werden nicht direkt im Kampf verwendet.

#### 7.2.1 Antriebsstärke

Die Antriebsstärke gibt an wie schnell ein Schiff sich bewegen kann. Die Angabe bezieht sich auf Distanzeinheiten pro
Stunde.

#### 7.2.2 Energieeffizienz

Die Energieeffizienz gibt an wieviel Elementium pro Distanzeinheit verbraucht wird.

#### 7.2.3 Lagerkapazität

Die Lagerkapazität gibt an wieviel Rohstoffe ein Schiff aufladen kann. Güter werden anhand ihres Levels umgerechnet.
Dabei gilt: Level des Guts = benötigter Platz im Lager.

Gekaperte Schiffe oder Verteidigungsanlagen verbrauchen keinen Platz im Lager, da diese mit einem Traktorstrahl
transportiert werden.

### 7.3 Basis Kampfablauf - Beispiel

Eine Zwergen Flotte greift einen Ork Planeten an. Auf dem Planeten sind 2 „Große manuelle Geschütze“ errichtet und im
Orbit halten sich 2 Aufklärer auf. Der Angreifer greift mit 5 Aufklärern an. Sämtliche Schiffe und
Verteidigungsanlagen sind nicht modifiziert und von der eigenen Rasse erbaut worden.

> **Ergänzung:** Abkürzungen im Beispiel: **AK** = Aufklärer, **GMG** = Großes manuelles Geschütz,
> **TG** = Treffgenauigkeit, **SS** = Schildstärke, **SP** = Strukturpunkte. Die Zahl in Klammern ist die
> Angriffsreihenfolge (Initiative) in der Runde. Werte aus dem Beispiel: AK CPU 9, Angriffsstärke 40, Panzerung 10;
> GMG CPU 10, Angriffsstärke 64.
>
> Ablauf einer Kampfrunde:
>
> ```mermaid
> flowchart TD
>     Start(["Kampfbeginn"]) --> P1
>     P1["Phase 1: Ziele anvisieren<br>Partei mit höherer gesamt CPU Leistung wählt zuerst"] --> P2
>     P2["Phase 2: Initiative festlegen<br>je Verbindung über die CPU Leistung"] --> P3
>     P3["Phase 3: Angreifen<br>in Initiative-Reihenfolge"] --> Q{"Kampf entschieden?"}
>     Q -- ja --> P4
>     Q -- nein --> R{"15 Kampfrunden erreicht?"}
>     R -- nein --> P1
>     R -- ja --> Rueckzug["Angreifer zieht sich zurück"] --> P4
>     P4["Phase 4: Nach dem Kampf<br>Trümmerfeld, Wiederherstellung, Beute"]
> ```

#### 7.3.1 Erste Kampf Phase => Ziele anvisieren

Jedes Objekt der Partei mit der stärkeren gesamt CPU Leistung nimmt ein zufälliges Ziel ins Visier. Erst wenn kein
freies Ziel mehr existiert werden Ziele doppelt, von zwei unterschiedlichen Schiffen anvisiert. Drei Schiffe visieren
ein einziges Ziel erst dann an, wenn alle Ziele bereits doppelt anvisiert sind, usw.

Sind nicht alle Ziele anvisiert können diese ihrerseits Schiffe doppelt oder dreifach, usw, ins Ziel nehmen.

Am Ende der ersten Kampfphase haben alle Schiffe ein oder mehrere Ziele im Visier und alle verteidigenden Schiffe und
Verteidigungsanlagen auch. Die Zuordnung ist zufällig. Anvisierte Ziele können in dieser Kampfrunde nicht mehr
gewechselt werden. Sind mehre Ziele im Visier wird eins davon zufällig für den Angriff bestimmt.

Die Zwergenflotte hat eine gesamt CPU Leistung von 45, die Orkflotte 38. Daher beginnt die Zwergenflotte mit der
Zielerfassung. Die Reihenfolge der Zwergenobjekte ist dabei zufällig. Hier sind es aber nur AKs, daher wird dies hier
nicht deutlich.

| Zwerge | Orks |
| --- | --- |
| AK Zwerg 1 | GMG Ork 1 |
| AK Zwerg 2<br>AK Zwerg 5 | AK Ork 1 |
| AK Zwerg 3 | GMG Ork 2 |
| AK Zwerg 4 | AK Ork 2 |

Da der Zwerg ein Schiff mehr hat als der Ork Schiffe + Verteidigungsanlagen kann der Zwerg ein Objekt des Orks doppelt
anvisieren.

#### 7.3.2 Zweite Kampf Phase => Initiative festlegen

Jede durch die erste Kampf Phase festgelegte Verbindung legt über die CPU Leistung die Angriffsreihenfolge fest.

Die Aufklärer haben CPU Leistung = 9 und die Großen manuellen Geschütze haben CPU Leistung = 10. Dadurch greifen zuerst
die GMGs an. Danach die AKs. Ist die CPU Leistung gleich (AK gegen AK), dann wird die Reihenfolge zufällig bestimmt.

| Zwerge | Orks |
| --- | --- |
| AK Zwerg 1, TG=45%, SS=20%, SP=50 (2) | GMG Ork 1, TG=50%, SS=25%, SP=80 (1) |
| AK Zwerg 2, TG=45%, SS=20%, SP=50 (3)<br>AK Zwerg 5, TG=45%, SS=20%, SP=50 (5) | AK Ork 1, TG=45%, SS=20%, SP=50 (4) |
| AK Zwerg 3, TG=45%, SS=20%, SP=50 (7) | GMG Ork 2, TG=50%, SS=25%, SP=80 (6) |
| AK Zwerg 4, TG=45%, SS=20%, SP=50 (9) | AK Ork 2, TG=45%, SS=20%, SP=50 (8) |

Die Reihenfolge zwischen (3), (4) und (5) sowie (8) und (9) sind zufällig bestimmt worden da alle beteiligten die
gleiche CPU Leistung haben.

#### 7.3.3 Dritte Kampf Phase => Angreifen

In der Reihenfolge der festgelegten Initiative greifen die Schiffe und Anlagen an. Treffer werden berechnet und Schaden
von den Strukturpunkten abgezogen. Bei Kampfunfähigkeit wird berechnet ob das Objekt zerstört oder erobert wurde. Nach
dem Ende einer Kampfrunde beginnt wieder Phase 1.

Ist ein Kampf nach 15 Kampfrunden nicht entschieden zieht sich der Angreifer zurück.

#### 7.3.4 Vierte Kampf Phase => Nach dem Kampf

Nach dem Kampf liegen auf der Koordinate des Kampfes Trümmer herum. Die Trümmer entsprechen den Rohstoffen die für das
Objekt ausgegeben wurden. Güter die gesockelt waren werden genauso zu Trümmern. Trümmer der Verteidigungsanlagen gehen
nicht in ein Trümmerfeld ein. Zerstörte Verteidigungsanlagen werden nach einem Kampf mit einer Wahrscheinlichkeit von
50% wiederhergestellt. Gesockelte Güter bleiben erhalten. Das gilt nicht für eroberte Verteidigungsanlagen.

Alle eroberten Objekte werden nach einem Kampf von dem Gewinner entweder mit zurück zum Ausgangsplaneten mitgenommen
oder auf dem Planeten direkt verbucht. Ist kein Platz für das eroberte Objekt vorhanden wird es zerstört. Das passiert
aber erst nachdem man am Ausgangsplaneten angekommen ist. Es bildet sich dann dort ein entsprechendes Trümmerfeld. Der
Spieler kann also während des Rückfluges noch für freien Platz sorgen.

> **Ergänzung:** Was nach dem Kampf mit jedem Objekt geschieht:
>
> ```mermaid
> flowchart TD
>     O["Objekt nach dem Kampf"] --> T{"Art und Zustand"}
>     T -- "Schiff zerstört" --> TF["Trümmerfeld an der Kampfkoordinate<br>(Baukosten + gesockelte Güter)"]
>     T -- "Verteidigungsanlage zerstört" --> W{"50 % Wiederherstellung"}
>     W -- ja --> WH["Wiederhergestellt<br>gesockelte Güter bleiben"]
>     W -- nein --> Weg["Verloren, kein Trümmerfeld"]
>     T -- "erobert" --> G{"Gewinner ist …"}
>     G -- Verteidiger --> Direkt["Direkt auf dem Planeten verbucht"]
>     G -- Angreifer --> Rueck["Rückflug zum Ausgangsplaneten<br>(braucht keinen Lagerplatz)"]
>     Direkt --> Platz{"Freier Platz?"}
>     Rueck --> Platz
>     Platz -- ja --> Integriert["In Flotte/Verteidigung übernommen"]
>     Platz -- nein --> TF2["Zerstört, Trümmerfeld auf diesem Planeten<br>(Angreifer: erst bei Ankunft)"]
> ```
>
> „Freier Platz“ meint vermutlich die Schiffsgrenze des Levels aus Kapitel 2; das Original sagt es nicht ausdrücklich.

#### 7.3.5 Beispiel

**Kampfrunde 1**

- GMG Ork 1 scheitert an der Treffgenauigkeit und erhält 10% Bonus auf die nächste Treffgenauigkeit Probe.
- AK Zwerg 1 scheitert an der Treffgenauigkeit und erhält 10% Bonus auf die nächste Treffgenauigkeit Probe.
- AK Zwerg 2 trifft AK Ork 1.
- AK Ork 1 verhindert den Treffer mit der Schildstärke und bekommt 10% Malus auf die nächste Schildstärke Probe.
- AK Ork 1 trifft AK Zwerg 2.
- AK Zwerg 2 scheitert an der Schildstärke Probe und kann den Treffer nicht verhindern.
- AK Ork 1 verursacht 40 Trefferpunkte energetischen Schaden. Der Schaden ist stark gegen strukturelle Rüstungen und
  erhöht sich auf 150% und verursacht 60 effektive Trefferpunkte. Die Rüstung von AK Zwerg 2 verringert die
  Trefferpunkte um 10 Punkte. AK Ork 1 verursacht an AK Zwerg 2 effektiv 50 Schadenspunkte die von den Strukturpunkten
  abgezogen werden.
- AK Ork 1 mach AK Zwerk 2 kapfunfähig. Die CPU Leistungen werden genutzt um die Kaperchance zu berechnen =>
  9 / ( 20 * 9 ) = 0,05 * 100% = 5%. AK Zwerg 2 wird von AK Ork 1 gekapert.
- AK Zwerg 5 trifft AK Ork 1.
- AK Ork 1 scheitert an der Schildstärke Probe und kann den Treffer nicht verhindern.
- AK Zwerg 5 verursacht 40 Trefferpunkte kinetischen Schaden. Der Schaden ist normal gegen kompensierende Rüstungen und
  verursacht 40 effektive Trefferpunkte. Die Rüstung von AK Ork 1 verringert die Trefferpunkte um 10 Punkte. AK Zwerg 5
  verursacht an AK Ork 1 effektiv 30 Schadenspunkte die von den Strukturpunkten abgezogen werden.
- AK Ork 1 hat jetzt noch 20 Strukturpunkte.
- GMG Ork 2 scheitert an der Treffgenauigkeit und erhält 10% Bonus auf die nächste Treffgenauigkeit Probe.
- AK Zwerg 3 scheitert an der Treffgenauigkeit und erhält 10% Bonus auf die nächste Treffgenauigkeit Probe.
- AK Ork 2 scheitert an der Treffgenauigkeit und erhält 10% Bonus auf die nächste Treffgenauigkeit Probe.
- AK Zwerg 4 trifft AK Ork 2.
- AK Ork 2 scheitert an der Schildstärke Probe und kann den Treffer nicht verhindern.
- AK Zwerg 4 verursacht 40 Trefferpunkte kinetischen Schaden. Der Schaden ist normal gegen kompensierende Rüstungen und
  verursacht 40 effektive Trefferpunkte. Die Rüstung von AK Ork 2 verringert die Trefferpunkte um 10 Punkte. AK Zwerg 4
  verursacht an AK Ork 2 effektiv 30 Schadenspunkte die von den Strukturpunkten abgezogen werden.
- AK Ork 2 hat jetzt noch 20 Strukturpunkte.
- Die Kampfrunde endet.

| Zwerge | Orks |
| --- | --- |
| AK Zwerg 1, TG=55%, SS=20%, SP=50 (2) | GMG Ork 1, TG=60%, SS=25%, SP=80 (1) |
| AK Zwerg 2, erobert (3)<br>AK Zwerg 5, TG=45%, SS=20%, SP=50 (5) | AK Ork 1, TG=45%, SS=10%, SP=20 (4) |
| AK Zwerg 3, TG=55%, SS=20%, SP=50 (7) | GMG Ork 2, TG=60%, SS=25%, SP=80 (6) |
| AK Zwerg 4, TG=45%, SS=20%, SP=50 (9) | AK Ork 2, TG=55%, SS=20%, SP=20 (8) |

**Kampfrunde 2**

Die Ziele werden neu anvisiert. Die gesamt CPU Leistung der Zwergenflotte ist 36, die der Orkflotte 38. Daher beginnt
die Orkflotte in dieser Runde mit der Zielerfassung. Die Reihenfolge der Orkobjekte ist dabei zufällig.

| Orks | Zwerge |
| --- | --- |
| AK Ork 1, TG=45%, SS=10%, SP=20 (2) | AK Zwerg 4, TG=45%, SS=20%, SP=50 (1) |
| GMG Ork 2, TG=60%, SS=25%, SP=80 (3) | AK Zwerg 1, TG=55%, SS=20%, SP=50 (4) |
| GMG Ork 1, TG=60%, SS=25%, SP=80 (5) | AK Zwerg 3, TG=55%, SS=20%, SP=50 (6) |
| AK Ork 2, TG=55%, SS=20%, SP=20 (7) | AK Zwerg 5, TG=45%, SS=20%, SP=50 (8) |
| | AK Zwerg 2, erobert |

Die Reihenfolge zwischen (1) und (2) sowie zwischen (7) und (8) ist zufällig bestimmt worden.

- AK Zwerg 4 scheitert an der Treffgenauigkeit und erhält 10% Bonus auf die nächste Treffgenauigkeit Probe.
- AK Ork 1 scheitert an der Treffgenauigkeit und erhält 10% Bonus auf die nächste Treffgenauigkeit Probe.
- GMG Ork 2 trifft AK Zwerg 1. Der Treffergenauigkeits Bonus schwindet.
- AK Zwerg 1 scheitert an der Schildstärke Probe und kann den Treffer nicht verhindern.
- GMG Ork 2 verursacht 64 Trefferpunkte energetischen Schaden. Der Schaden ist stark gegen strukturelle Rüstungen und
  erhöht sich auf 150% und verursacht 96 effektive Trefferpunkte. Die Rüstung von AK Zwerg 1 verringert die
  Trefferpunkte um 10 Punkte. GMG Ork 2 verursacht an AK Zwerg 1 effektiv 86 Schadenspunkte die von den
  Strukturpunkten abgezogen werden.
- GMG Ork 2 mach AK Zwerk 1 kapfunfähig. Die CPU Leistungen werden genutzt um die Kaperchance zu berechnen =>
  10 / ( 20 * 9 ) = 0,0555 * 100% = 5,55%. AK Zwerg 1 wird von GMG Ork 2 zerstört.
- GMG Ork 1 trifft AK Zwerg 3. Der Treffergenauigkeits Bonus schwindet.
- AK Zwerg 3 scheitert an der Schildstärke Probe und kann den Treffer nicht verhindern.
- GMG Ork 1 verursacht 64 Trefferpunkte energetischen Schaden. Der Schaden ist stark gegen strukturelle Rüstungen und
  erhöht sich auf 150% und verursacht 96 effektive Trefferpunkte. Die Rüstung von AK Zwerg 3 verringert die
  Trefferpunkte um 10 Punkte. GMG Ork 1 verursacht an AK Zwerg 3 effektiv 86 Schadenspunkte die von den
  Strukturpunkten abgezogen werden.
- GMG Ork 1 mach AK Zwerg 3 kapfunfähig. Die CPU Leistungen werden genutzt um die Kaperchance zu berechnen =>
  10 / ( 20 * 9 ) = 0,0555 * 100% = 5,55%. AK Zwerg 3 wird von GMG Ork 1 zerstört.
- AK Ork 2 scheitert an der Treffgenauigkeit und erhält 10% Bonus auf die nächste Treffgenauigkeit Probe.
- AK Zwerg 5 trifft AK Ork 2.
- AK Ork 2 scheitert an der Schildstärke Probe und kann den Treffer nicht verhindern.
- AK Zwerg 5 verursacht 40 Trefferpunkte kinetischen Schaden. Der Schaden ist normal gegen kompensierende Rüstungen und
  verursacht 40 effektive Trefferpunkte. Die Rüstung von AK Ork 2 verringert die Trefferpunkte um 10 Punkte. AK Zwerg 5
  verursacht an AK Ork 2 effektiv 30 Schadenspunkte die von den Strukturpunkten abgezogen werden.
- AK Zwerg 5 mach AK Ork 2 kapfunfähig. Die CPU Leistungen werden genutzt um die Kaperchance zu berechnen =>
  9 / ( 20 * 9 ) = 0,05 * 100% = 5%. AK Ork 2 wird von AK Zwerg 5 zerstört.

| Orks | Zwerge |
| --- | --- |
| AK Ork 1, TG=55%, SS=10%, SP=20 (2) | AK Zwerg 4, TG=55%, SS=20%, SP=50 (1) |
| GMG Ork 2, TG=50%, SS=25%, SP=80 (3) | AK Zwerg 1, zerstört (4) |
| GMG Ork 1, TG=50%, SS=25%, SP=80 (5) | AK Zwerg 3, zerstört (6) |
| AK Ork 2, zerstört (7) | AK Zwerg 5, TG=45%, SS=20%, SP=50 (8) |
| | AK Zwerg 2, erobert |

**Kampfrunde 3**

Die Ziele werden neu anvisiert. Die gesamt CPU Leistung der Zwergenflotte ist 18, die der Orkflotte 29. Daher beginnt
die Orkflotte in dieser Runde mit der Zielerfassung. Die Reihenfolge der Orkobjekte ist dabei zufällig.

| Orks | Zwerge |
| --- | --- |
| GMG Ork 2, TG=50%, SS=25%, SP=80 (1)<br>AK Ork 1, TG=55%, SS=10%, SP=20 (3) | AK Zwerg 4, TG=55%, SS=20%, SP=50 (2) |
| GMG Ork 1, TG=60%, SS=25%, SP=80 (4) | AK Zwerg 5, TG=45%, SS=20%, SP=50 (5) |
| AK Ork 2, zerstört | AK Zwerg 2, erobert<br>AK Zwerg 1, zerstört<br>AK Zwerg 3, zerstört |

Die Reihenfolge zwischen (2) und (3) ist zufällig bestimmt worden.

- GMG Ork 2 trifft AK Zwerg 4.
- AK Zwerg 4 verhindert den Treffer mit der Schildstärke und bekommt 10% Malus auf die nächste Schildstärke Probe.
- AK Zwerg 4 trifft AK Ork 1
- AK Ork 1 scheitert an der Schildstärke Probe und kann den Treffer nicht verhindern.
- AK Zwerg 4 verursacht 40 Trefferpunkte kinetischen Schaden. Der Schaden ist normal gegen kompensierende Rüstungen und
  verursacht 40 effektive Trefferpunkte. Die Rüstung von AK Ork 1 verringert die Trefferpunkte um 10 Punkte. AK Zwerg 4
  verursacht an AK Ork 1 effektiv 30 Schadenspunkte die von den Strukturpunkten abgezogen werden.
- AK Ork 1 hat jetzt noch 20 Strukturpunkte.
- AK Zwerg 4 mach AK Ork 1 kapfunfähig. Die CPU Leistungen werden genutzt um die Kaperchance zu berechnen =>
  9 / ( 20 * 9 ) = 0,05 * 100% = 5%. AK Ork 1 wird von AK Zwerg 4 zerstört.
- GMG Ork 1 trifft AK Zwerg 5. Der Treffergenauigkeits Bonus schwindet.
- AK Zwerg 5 scheitert an der Schildstärke Probe und kann den Treffer nicht verhindern.
- GMG Ork 1 verursacht 64 Trefferpunkte energetischen Schaden. Der Schaden ist stark gegen strukturelle Rüstungen und
  erhöht sich auf 150% und verursacht 96 effektive Trefferpunkte. Die Rüstung von AK Zwerg 5 verringert die
  Trefferpunkte um 10 Punkte. GMG Ork 1 verursacht an AK Zwerg 5 effektiv 86 Schadenspunkte die von den
  Strukturpunkten abgezogen werden.
- GMG Ork 1 mach AK Zwerg 5 kapfunfähig. Die CPU Leistungen werden genutzt um die Kaperchance zu berechnen =>
  10 / ( 20 * 9 ) = 0,0555 * 100% = 5,55%. AK Zwerg 5 wird von GMG Ork 1 zerstört.

| Orks | Zwerge |
| --- | --- |
| GMG Ork 2, TG=50%, SS=25%, SP=80 (1)<br>AK Ork 1, zerstört (3) | AK Zwerg 4, TG=55%, SS=10%, SP=50 (2) |
| GMG Ork 1, TG=50%, SS=25%, SP=80 (4) | AK Zwerg 5, TG=45%, SS=20%, SP=50 (5) |
| AK Ork 2, zerstört | AK Zwerg 2, erobert<br>AK Zwerg 1, zerstört<br>AK Zwerg 3, zerstört |

**Kampfrunde 4**

Die Ziele werden neu anvisiert. Die gesamt CPU Leistung der Zwergenflotte ist 18, die der Orkflotte 20. Daher beginnt
die Orkflotte in dieser Runde mit der Zielerfassung. Die Reihenfolge der Orkobjekte ist dabei zufällig.

In dieser Kampfrunde werden die Zwergenschiffe vernichtet.

Auf dem Ork Planet wird das eroberte Zwergenschiff in die bestehende Flotte integriert soweit dafür ein freier Platz
vorhanden ist. Ansonsten wird das Schiff wie die anderen 4 Zwergenschiffe zerstört.

Die Rohstoffe die für die Konstruktion der zerstörten Schiffe bezahlt wurden liegen jetzt als Trümmerfeld auf den
Koordinaten des Planeten.

> **Ergänzung:** Kleine Unstimmigkeiten im Originalbeispiel, unverändert übernommen:
>
> - Runde 3: GMG Ork 1 startet mit TG=60 %, obwohl er in Runde 2 getroffen hat und sein Bonus geschwunden ist (50 %).
> - Runde 3: „AK Ork 1 hat jetzt noch 20 Strukturpunkte“ steht vor seiner Zerstörung; rechnerisch hat er
>   20 − 30 = −10. Die Zeile wurde vermutlich aus Runde 1 kopiert. Zerstört ist er trotzdem zu Recht.
> - Runde 3: In der Tabelle danach steht AK Zwerg 5 noch mit SP=50, obwohl er zerstört wurde. Entsprechend nennt
>   Runde 4 für die Zwerge 18 CPU Leistung, obwohl nur noch AK Zwerg 4 (9) übrig ist.
> - CPU-Summen sonst stimmig: 5 AK × 9 = 45 (Zwerge) und 2 × 9 + 2 × 10 = 38 (Orks); in Runde 2 zählen die 36 der
>   Zwerge den eroberten AK Zwerg 2 nicht mehr mit.

## 8 Gebäude

### 8.1 Rohstoff Gebäude

Um Rohstoffe zu fördern müssen fördernde Gebäude gebaut werden. Alle Rohstoffe werden gesammelt und steigen
kontinuierlich entsprechend ihrer aktuellen Billanz aus Ertrag und Verbrauch an oder ab. Verbrauch und Ertrag werden
auf 24h angegeben und müssen daher beim Verbuchen des aktuellen Standes entsprechend umgerechnet werden.

Basis Bau- und Betriebskosten steigen mit dem Level des Gebäudes. Siehe dazu [Wachstumsraten](#6-wachstumsraten).

Folgende Gebäude sind in Galaxxis verfügbar:

> **Ergänzung:** Die Zahlen dieses Kapitels sind von 2015. Verbindlich sind die neueren Werte aus
> [`ConzeptExcel.fods`](ConzeptExcel.fods) (Blatt „Kosten und Erträge“), umgesetzt in
> `src/Galaxxis.Domain/Buildings`. Zusätzlich gelten die Versorgungsregeln aus der `CLAUDE.md` (Verbrauch nur während
> der Produktion, Produktionsgeschwindigkeit in 10-%-Schritten, anteilige Drosselung bei leerem Versorgungsrohstoff).
>
> Welche Versorgungsrohstoffe jedes Gebäude im Betrieb verbraucht (nach den Tabellen unten; jedes Gebäude verbraucht
> nie seinen eigenen Ertrag):
>
> ```mermaid
> flowchart LR
>     Energie -.-> Erzmine
>     Mana -.-> Erzmine
>     Elementium -.-> Erzmine
>     Mana -.-> Kraftwerk
>     Antimaterie -.-> Kraftwerk
>     Elementium -.-> Kraftwerk
>     Energie -.-> Fabrik
>     Antimaterie -.-> Fabrik
>     Elementium -.-> Fabrik
>     Energie -.-> Magier["Magier Akademie"]
>     Antimaterie -.-> Magier
>     Elementium -.-> Magier
>     Mana -.-> Kristallmine
>     Antimaterie -.-> Kristallmine
>     Elementium -.-> Kristallmine
>     Energie -.-> Neutronen["Neutronenbeschleuniger"]
>     Mana -.-> Neutronen
>     Elementium -.-> Neutronen
>     Energie -.-> Synth["Elementium Synthetisierer"]
>     Mana -.-> Synth
>     Antimaterie -.-> Synth
> ```

#### 8.1.1 Erzmine

| Erzmine | |
| --- | --- |
| Basis Baukosten<br>einmalig | Erze: 10<br>Mana: 10<br>Kristalle: 5<br>Elementium: 0 |
| Basis Betriebskosten<br>pro 24h | Energie: 5<br>Mana: 5<br>Antimaterie: 0<br>Elementium: 5 |
| Basis Ertrag<br>pro 24h | 150 Erze |
| Grundertrag<br>pro 24h | 50 Erze |
| Wachstumsfaktor | Errichten 4<br>Verbrauch 2<br>Ertrag 1 |

#### 8.1.2 Kraftwerk

| Kraftwerk | |
| --- | --- |
| Basis Baukosten<br>einmalig | Erze: 15<br>Mana: 5<br>Kristalle: 5<br>Elementium: 0 |
| Basis Betriebskosten<br>pro 24h | Energie: 0<br>Mana: 5<br>Antimaterie: 5<br>Elementium: 5 |
| Basis Ertrag<br>pro 24h | 150 Energie |
| Grundertrag<br>pro 24h | 50 Energie |
| Wachstumsfaktor | Errichten 4<br>Verbrauch 2<br>Ertrag 1 |

#### 8.1.3 Fabrik

| Fabrik | |
| --- | --- |
| Basis Baukosten<br>einmalig | Erze: 10<br>Mana: 5<br>Kristalle: 10<br>Elementium: 0 |
| Basis Betriebskosten<br>pro 24h | Energie: 5<br>Mana: 0<br>Antimaterie: 5<br>Elementium: 5 |
| Basis Ertrag<br>pro 24h | 150 Werkzeuge |
| Grundertrag<br>pro 24h | 50 Werkzeuge |
| Wachstumsfaktor | Errichten 4<br>Verbrauch 2<br>Ertrag 1 |

#### 8.1.4 Magier Akademie

| Magier Akademie | |
| --- | --- |
| Basis Baukosten<br>einmalig | Erze: 15<br>Mana: 15<br>Kristalle: 10<br>Elementium: 5 |
| Basis Betriebskosten<br>pro 24h | Energie: 15<br>Mana: 0<br>Antimaterie: 10<br>Elementium: 5 |
| Basis Ertrag<br>pro 24h | 125 Mana |
| Grundertrag<br>pro 24h | 25 Mana |
| Wachstumsfaktor | Errichten 4<br>Verbrauch 2<br>Ertrag 1 |

#### 8.1.5 Kristallmine

| Kristallmine | |
| --- | --- |
| Basis Baukosten<br>einmalig | Erze: 15<br>Mana: 15<br>Kristalle: 10<br>Elementium: 5 |
| Basis Betriebskosten<br>pro 24h | Erze: 0<br>Mana: 10<br>Antimaterie: 15<br>Elementium: 5 |
| Basis Ertrag<br>pro 24h | 125 Kristalle |
| Grundertrag<br>pro 24h | 25 Kristalle |
| Wachstumsfaktor | Errichten 4<br>Verbrauch 2<br>Ertrag 1 |

> **Ergänzung:** Bei den Betriebskosten steht im Original „Erze: 0“; gemeint ist vermutlich „Energie: 0“, wie bei
> allen anderen Gebäuden (Erz ist kein Versorgungsrohstoff).

#### 8.1.6 Neutronenbeschleuniger

| Neutronenbeschleuniger | |
| --- | --- |
| Basis Baukosten<br>einmalig | Erze: 20<br>Mana: 10<br>Kristalle: 10<br>Elementium: 5 |
| Basis Betriebskosten<br>pro 24h | Energie: 10<br>Mana: 15<br>Antimaterie: 0<br>Elementium: 5 |
| Basis Ertrag<br>pro 24h | 125 Antimaterie |
| Grundertrag<br>pro 24h | 25 Antimaterie |
| Wachstumsfaktor | Errichten 4<br>Verbrauch 2<br>Ertrag 1 |

#### 8.1.7 Elementium Synthetisierer

| Neutronenbeschleuniger | |
| --- | --- |
| Basis Baukosten<br>einmalig | Erze: 25<br>Mana: 20<br>Kristalle: 20<br>Elementium: 10 |
| Basis Betriebskosten<br>pro 24h | Energie: 15<br>Mana: 15<br>Antimaterie: 15<br>Elementium: 0 |
| Basis Ertrag<br>pro 24h | 60 Elementium |
| Grundertrag<br>pro 24h | 10 Elementium |
| Wachstumsfaktor | Errichten 4<br>Verbrauch 2<br>Ertrag 1 |

> **Ergänzung:** Die Tabellenüberschrift lautet im Original „Neutronenbeschleuniger“ (aus 8.1.6 kopiert); die Werte
> gehören zum Elementium Synthetisierer.
