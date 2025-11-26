# WarfarinManager Pro - Implementazione Tab "Storico INR"

## 📋 CONTESTO PROGETTO

Sto sviluppando **WarfarinManager Pro**, un'applicazione desktop WPF per medici di medicina generale italiani, per la gestione della terapia anticoagulante orale con warfarin secondo le linee guida FCSA-SIMG.

### Stato Attuale del Progetto ✅

**Architettura Clean Architecture implementata**:
```
D:\Claude\winTaoGest/
├── src/
│   ├── WarfarinManager.Shared/      ✅ Enums, Constants
│   ├── WarfarinManager.Data/        ✅ EF Core, SQLite, Repositories, 11 Entities
│   ├── WarfarinManager.Core/        ✅ Business Logic (DosageCalculator, TTRCalculator, InteractionChecker)
│   └── WarfarinManager.UI/          ✅ WPF MVVM (parzialmente completato)
└── tests/
    └── WarfarinManager.Tests/       ✅ Unit + Integration tests
```

**Tab già implementati nell'applicazione**:
- ✅ Tab "Anagrafica" - Visualizzazione dati paziente
- ✅ Tab "Indicazione alla TAO" - CRUD indicazioni terapeutiche con target INR
- ✅ Tab "Farmaci" - Gestione farmaci concomitanti con alert interazioni warfarin
- ✅ Tab "Bridge Therapy" - Protocollo perioperatorio con export PDF
- ⏳ Tab "Storico INR" - **DA IMPLEMENTARE** (placeholder attuale)
- ⏳ Tab "Eventi Avversi" - placeholder

**Componenti correlati già esistenti**:
- ✅ `INRControlView.xaml` - Form per inserimento nuovo controllo INR (in Views/INR/)
- ✅ `INRChartView.xaml` + `INRChartViewModel.cs` - Grafico andamento INR con LiveCharts2
- ✅ `INRControlDto` - DTO completo con tutte le proprietà necessarie
- ✅ `TTRCalculatorService` - Calcolo TTR con metodo Rosendaal

---

## 🎯 OBIETTIVO

Implementare il **Tab "Storico INR"** nella `PatientDetailsView`, che deve contenere:

1. **Tabella storico controlli INR** (parte superiore)
2. **Grafico andamento INR** (parte inferiore, utilizzando `INRChartView` esistente)
3. **Indicatore TTR** (prominente, con colore basato su qualità)

---

## 📐 LAYOUT RICHIESTO

```
┌─────────────────────────────────────────────────────────────────────┐
│ TOOLBAR                                                              │
│ [⏱ 3 mesi] [⏱ 6 mesi] [⏱ 12 mesi] [⏱ Tutto]    [📊 Esporta CSV]   │
├─────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  ┌─────────────────────────────────┐  ┌──────────────────────────┐  │
│  │ INDICATORE TTR                  │  │ STATISTICHE              │  │
│  │ ┌─────────────────────────────┐ │  │ N° Controlli: 24         │  │
│  │ │        TTR: 72%             │ │  │ INR Medio: 2.45          │  │
│  │ │       (Eccellente)          │ │  │ Deviazione Std: 0.38     │  │
│  │ │    ████████████░░░░         │ │  │ In Range: 75%            │  │
│  │ └─────────────────────────────┘ │  │ Ultimo INR: 2.3 (15/11)  │  │
│  └─────────────────────────────────┘  └──────────────────────────┘  │
│                                                                      │
├─────────────────────────────────────────────────────────────────────┤
│ TABELLA STORICO INR                                          ▲      │
│ ┌──────────┬───────┬────────┬──────────┬──────────┬────────┐ │      │
│ │ Data     │ INR   │ Status │ Dose Sett│ Variaz.% │ Fase   │ │      │
│ ├──────────┼───────┼────────┼──────────┼──────────┼────────┤ │      │
│ │15/11/2025│ 2.30  │ ✓ Range│ 35.0 mg  │    -     │ Manten.│ │      │
│ │01/11/2025│ 2.85  │ ✓ Range│ 35.0 mg  │  +7.7%   │ Manten.│ │      │
│ │15/10/2025│ 1.75  │ ⚠ Sotto│ 32.5 mg  │  -7.1%   │ Post-A │ │      │
│ │01/10/2025│ 3.40  │ ⚠ Sopra│ 35.0 mg  │  +14.3%  │ Manten.│ │      │
│ │...       │ ...   │ ...    │ ...      │ ...      │ ...    │ ▼      │
│ └──────────┴───────┴────────┴──────────┴──────────┴────────┘        │
├─────────────────────────────────────────────────────────────────────┤
│ GRAFICO ANDAMENTO INR                                               │
│ ┌─────────────────────────────────────────────────────────────────┐ │
│ │  INR │                                                          │ │
│ │  4.0 │─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ │ │
│ │  3.5 │████████████████████████████████████████████████████████│ │
│ │  3.0 │═══════════════════════════════════════════════════════│ │ │
│ │  2.5 │         ●───●                    ●───●                 │ │
│ │  2.0 │═══════════════════════════════════════════════════════│ │ │
│ │  1.5 │████████████████████████████████████████████████████████│ │
│ │      └────┬────┬────┬────┬────┬────┬────┬────┬────┬────┬────  │ │
│ │         Ott  Nov  Dic  Gen  Feb  Mar  Apr  Mag  Giu  Lug       │ │
│ └─────────────────────────────────────────────────────────────────┘ │
│ Legenda: ═══ Range target (2.0-3.0)  ● In range  ● Fuori range      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 📊 SPECIFICHE FUNZIONALI

### 1. Tabella Storico INR

**Colonne DataGrid**:
| Colonna | Binding | Larghezza | Note |
|---------|---------|-----------|------|
| Data | `FormattedDate` | 100 | Ordinamento default DESC |
| INR | `INRValue` | 80 | Format "F2", colore basato su status |
| Status | `Status` | 100 | "✓ In Range", "⚠ Sotto", "⚠ Sopra" con colore |
| Dose Sett. | `CurrentWeeklyDose` | 100 | Format "{0:F1} mg" |
| Variazione | Calcolata | 90 | % rispetto controllo precedente |
| Fase | `PhaseDescription` | 100 | "Induzione", "Mantenimento", "Post-agg." |
| Giorni | Calcolati | 80 | Giorni dal controllo precedente |
| Note | `Notes` | * | Troncate con tooltip |

**Funzionalità tabella**:
- Ordinamento per colonna (default: data DESC)
- Selezione riga → evidenzia punto corrispondente nel grafico
- Double-click → apre dettaglio controllo (opzionale)
- Row style: righe fuori range con sfondo colorato leggero

### 2. Indicatore TTR

**Componente prominente** con:
- Percentuale TTR grande (es. 72%)
- Barra di progresso colorata
- Label qualità: "Eccellente" (≥70%), "Accettabile" (60-69%), "Subottimale" (50-59%), "Critico" (<50%)
- Colori: Verde (#107C10), Giallo (#FFB900), Arancione (#FF8C00), Rosso (#E81123)

### 3. Box Statistiche

Mostrare:
- N° Controlli totali (nel periodo selezionato)
- INR Medio
- Deviazione Standard
- % In Range
- Ultimo controllo INR (valore + data)

### 4. Filtro Temporale

Pulsanti toggle per filtrare:
- 3 mesi
- 6 mesi (default)
- 12 mesi
- Tutto lo storico

Il filtro deve aggiornare: tabella, grafico, statistiche, TTR

### 5. Export CSV

Pulsante per esportare tabella in CSV con tutte le colonne.

---

## 🗂️ FILE ESISTENTI DA RIUTILIZZARE

### INRChartView.xaml (già funzionante)
Percorso: `src/WarfarinManager.UI/Views/Charts/INRChartView.xaml`

Caratteristiche:
- Grafico LiveCharts2 con linea INR
- Punti colorati (verde in range, rosso fuori range)
- Click su punto → mostra dettagli nel pannello laterale
- Statistiche base (N° controlli, INR medio, % in range)
- Supporta già filtro temporale via `SetTimeRangeCommand`

### INRChartViewModel.cs (già funzionante)
Percorso: `src/WarfarinManager.UI/ViewModels/INRChartViewModel.cs`

Metodi principali:
```csharp
public void LoadData(IEnumerable<INRControlDto> controls, decimal targetMin, decimal targetMax)
public void UpdateTTR(decimal ttrValue)
public void OnChartPointClicked(DateTime date)
[RelayCommand] private void SetTimeRange(string monthsStr)
```

### INRControlDto (già completo)
Percorso: `src/WarfarinManager.UI/Models/INRControlDto.cs`

Proprietà rilevanti:
- `ControlDate`, `INRValue`, `CurrentWeeklyDose`
- `TargetINRMin`, `TargetINRMax`
- `IsInRange`, `Status`, `StatusColor`
- `Phase`, `PhaseDescription`
- `FormattedDate`

### TTRCalculatorService (già funzionante)
Percorso: `src/WarfarinManager.Core/Services/TTRCalculatorService.cs`

```csharp
public interface ITTRCalculatorService
{
    TTRResult CalculateTTR(IEnumerable<INRControlDto> controls, decimal targetMin, decimal targetMax);
}
```

Restituisce: `TTRPercentage`, `DaysInRange`, `TotalDays`, `QualityLevel`

---

## 🏗️ STRUTTURA DA CREARE

### Nuovi file:
```
src/WarfarinManager.UI/
├── Views/Patient/
│   ├── INRHistoryView.xaml        ← NUOVO (UserControl)
│   └── INRHistoryView.xaml.cs     ← NUOVO
├── ViewModels/
│   └── INRHistoryViewModel.cs     ← NUOVO
```

### Modifiche a file esistenti:
- `PatientDetailsView.xaml` → Sostituire placeholder Tab 4 con `<local:INRHistoryView>`
- `PatientDetailsViewModel.cs` → Aggiungere `INRHistoryViewModel` e caricamento dati

---

## 📦 DIPENDENZE GIÀ INSTALLATE

```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
<PackageReference Include="LiveChartsCore.SkiaSharpView.WPF" Version="2.0.0-rc2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<PackageReference Include="QuestPDF" Version="2024.10.2" />
```

---

## 🎨 STILE UI

**Colori corporate** (da usare consistentemente):
- Primary Blue: `#0078D4`
- Success Green: `#107C10`
- Warning Yellow: `#FFB900`
- Warning Orange: `#FF8C00`
- Error Red: `#E81123` / `#D13438`
- Light Gray Background: `#F5F5F5`
- Border Gray: `#E0E0E0`
- Text Dark: `#333333`
- Text Light: `#666666`

**Font**: Segoe UI, minimo 11pt
**Lingua UI**: Italiano

---

## ✅ ACCEPTANCE CRITERIA

1. [ ] Tab "Storico INR" visualizza tabella completa dei controlli
2. [ ] Indicatore TTR prominente con colore qualità
3. [ ] Grafico INRChartView integrato correttamente
4. [ ] Filtri temporali funzionanti (3/6/12 mesi, tutto)
5. [ ] Selezione riga tabella ↔ evidenzia punto grafico (sincronizzazione)
6. [ ] Export CSV funzionante
7. [ ] Calcolo variazione % dose rispetto controllo precedente
8. [ ] Statistiche aggiornate al cambio filtro temporale
9. [ ] Performance: caricamento <1s per 100 controlli
10. [ ] Compilazione senza errori

---

## 💻 COMANDI UTILI

**Compilazione**:
```powershell
dotnet build D:\Claude\winTaoGest\WarfarinManager.sln
```

**Esecuzione**:
```powershell
dotnet run --project D:\Claude\winTaoGest\src\WarfarinManager.UI
```

---

## 📚 RIFERIMENTI PRD

Dal PRD originale (sezione 3.5):

> **Storico INR e TTR**
> 
> **Vista Tabellare Storico**: Tabella con colonne Data prelievo, INR, In range (✓/✗), Dose settimanale (mg), Variazione dose vs precedente (%), Giorni dal controllo precedente, Fase terapia, Note.
> 
> Funzionalità: Ordinamento per colonna, Filtro per range date, Ricerca testuale, Export CSV.
> 
> **Calcolo TTR (Time in Therapeutic Range)** - Metodo Rosendaal (interpolazione lineare).
> 
> **Visualizzazione TTR**:
> - Percentuale globale sempre visibile nella dashboard paziente
> - Indicatore colorato: Verde ≥70%, Giallo 60-69%, Arancione 50-59%, Rosso <50%
> - TTR periodo selezionato: ultimi 3/6/12 mesi

---

## 🚀 READY TO START

Inizia con:
1. Creare `INRHistoryViewModel.cs` con logica caricamento e filtri
2. Creare `INRHistoryView.xaml` con layout completo
3. Integrare in `PatientDetailsView.xaml`
4. Testare sincronizzazione tabella ↔ grafico
