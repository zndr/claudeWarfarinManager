# Risoluzione Problema: Tab "Valutazione Pre-TAO" non visibile

## Problema
Il tab "🩺 Valutazione Pre-TAO" non appare nell'applicazione anche dopo il rebuild.

## Causa
Visual Studio potrebbe utilizzare file compilati in cache o la directory bin/obj potrebbe contenere file obsoleti.

## Soluzione Passo-Passo

### 1. Chiudi Visual Studio Completamente
- Chiudi tutte le istanze di Visual Studio
- Assicurati che non ci siano processi `devenv.exe` attivi nel Task Manager

### 2. Pulizia Manuale (DA TERMINALE)

```bash
# Vai nella directory del progetto
cd C:\Users\ZANDA\.claude-worktrees\winTaoGest\sweet-roentgen

# Pulisci tutte le cartelle bin e obj
dotnet clean WarfarinManager.sln

# Rimuovi manualmente le cartelle (opzionale ma raccomandato)
rm -rf src/WarfarinManager.UI/bin
rm -rf src/WarfarinManager.UI/obj
```

### 3. Rebuild Completo

```bash
# Rebuild dell'intera soluzione
dotnet build WarfarinManager.sln
```

### 4. Avvia l'Applicazione

**Opzione A: Da terminale (raccomandato per test)**
```bash
cd src/WarfarinManager.UI
dotnet run
```

**Opzione B: Da Visual Studio**
- Apri Visual Studio
- Apri la soluzione WarfarinManager.sln
- Premi F5 (o Start Debugging)

## Verifica che il Tab Sia Presente

Quando l'applicazione si avvia:

1. **Apri un paziente esistente** dalla lista
2. **Conta i tab** nella vista Dettagli Paziente - dovrebbero essere **7**:
   - 📋 Anagrafica
   - **🩺 Valutazione Pre-TAO** ← QUESTO È IL NUOVO TAB
   - 🎯 Indicazione alla TAO
   - 💊 Farmaci
   - 📊 Storico INR
   - 💉 Bridge Therapy
   - ⚠ Eventi Avversi

3. **Clicca sul tab "🩺 Valutazione Pre-TAO"**
4. Dovresti vedere:
   - Dashboard con 3 pannelli (CHA₂DS₂-VASc, HAS-BLED, Valutazione)
   - Griglia a 2 colonne con checkbox

## Se il Tab Ancora Non Appare

### Controlla i Log dell'Applicazione

I log si trovano in:
```
C:\Users\ZANDA\AppData\Local\WarfarinManager\Logs\
```

Cerca il file più recente (`app{DATA}.log`) e verifica se ci sono errori tipo:
- `PreTaoAssessmentViewModel è NULL!`
- Errori di inizializzazione
- Eccezioni durante il caricamento

### Verifica il Database

La tabella deve esistere. Esegui:

```bash
cd TestMigration
dotnet run
```

Output atteso:
```
✅ PreTaoAssessments table exists! Count: 0
```

Se vedi `❌ PreTaoAssessments table error`, significa che la migrazione non è stata applicata.

### Applica Manualmente la Migrazione

Se necessario:

```bash
cd TestMigration
dotnet run
```

Questo script:
1. Controlla le migrazioni pending
2. Le applica automaticamente
3. Verifica che la tabella esista

## Verifica Tecnica dei File

### File che DEVONO esistere:

```bash
src/WarfarinManager.Data/Entities/PreTaoAssessment.cs
src/WarfarinManager.Data/Configuration/PreTaoAssessmentConfiguration.cs
src/WarfarinManager.Data/Migrations/20251127000000_AddPreTaoAssessment.cs
src/WarfarinManager.Data/Migrations/20251127000000_AddPreTaoAssessment.Designer.cs
src/WarfarinManager.UI/ViewModels/PreTaoAssessmentViewModel.cs
src/WarfarinManager.UI/Views/Patient/PreTaoAssessmentView.xaml
src/WarfarinManager.UI/Views/Patient/PreTaoAssessmentView.xaml.cs
```

Verifica con:
```bash
ls src/WarfarinManager.UI/Views/Patient/PreTaoAssessment*
```

### Il Tab DEVE essere nel XAML:

```bash
grep "Valutazione Pre-TAO" src/WarfarinManager.UI/Views/Patient/PatientDetailsView.xaml
```

Output atteso:
```
330:            <!--  Tab 2: Valutazione Pre-TAO  -->
331:            <TabItem Header="🩺 Valutazione Pre-TAO">
```

### ViewModel DEVE essere registrato:

```bash
grep "PreTaoAssessmentViewModel" src/WarfarinManager.UI/App.xaml.cs
```

Output atteso:
```
services.AddTransient<PreTaoAssessmentViewModel>();
```

## Ultima Risorsa: Reset Completo

Se nulla funziona:

1. **Backup del database**:
```bash
copy C:\Users\ZANDA\AppData\Local\WarfarinManager\warfarin.db C:\Users\ZANDA\AppData\Local\WarfarinManager\warfarin.db.backup
```

2. **Pulizia totale**:
```bash
dotnet clean WarfarinManager.sln
rm -rf src/*/bin
rm -rf src/*/obj
rm -rf TestMigration/bin
rm -rf TestMigration/obj
```

3. **Rebuild da zero**:
```bash
dotnet restore WarfarinManager.sln
dotnet build WarfarinManager.sln
```

4. **Applica migrazione**:
```bash
cd TestMigration
dotnet run
```

5. **Esegui app**:
```bash
cd ../src/WarfarinManager.UI
dotnet run
```

## Test Rapido: Screenshot

Quando vedi il tab, fai uno screenshot e inviamelo per confermare che tutto è ok!

Il tab dovrebbe apparire così:
- **Header**: "🩺 Valutazione Pre-TAO"
- **Posizione**: Secondo tab (dopo Anagrafica)
- **Contenuto**: Dashboard con score + griglia checkbox

## Supporto

Se dopo tutti questi passaggi il tab ancora non appare:
1. Inviami i log dell'applicazione
2. Inviami uno screenshot della vista Dettagli Paziente
3. Esegui e inviami l'output di:
```bash
cd TestMigration && dotnet run
```

---

**Nota**: È FONDAMENTALE che Visual Studio sia completamente chiuso durante la pulizia manuale di bin/obj, altrimenti potrebbe ricreare i file immediatamente.
