# 📝 Guida: Aggiornamento Versione e Changelog

Questa guida descrive tutti i file che devono essere aggiornati quando si rilascia una nuova versione di TaoGEST.

## 📋 Checklist File da Aggiornare

Quando rilasci la versione **X.Y.Z** (es. 1.2.0):

### 1. ✅ Version.props
**Percorso**: `Version.props`

Aggiorna:
```xml
<VersionPrefix>1.2.0</VersionPrefix>
<AssemblyVersion>1.2.0.0</AssemblyVersion>
<FileVersion>1.2.0.0</FileVersion>
```

### 2. ✅ Installer Inno Setup
**Percorso**: `installer/TaoGEST-Setup.iss`

Aggiorna:
```iss
; Versione 1.2.0

#define MyAppVersion "1.2.0"
```

### 3. ✅ MainWindow.xaml - Testo Versione (Angolo Basso a Sinistra)
**Percorso**: `src/WarfarinManager.UI/MainWindow.xaml`

Cerca la riga ~177 e aggiorna:
```xml
<TextBlock
    Grid.Row="1"
    Margin="10,0"
    VerticalAlignment="Center"
    Foreground="Gray"
    Text="Versione 1.2.0" />
```

### 4. ✅ AboutDialog.xaml - Tab "Novità"
**Percorso**: `src/WarfarinManager.UI/Views/Dialogs/AboutDialog.xaml`

#### 4a. Aggiungi Nuovo Blocco Changelog (linea ~104)

Inserisci PRIMA del blocco della versione precedente:

```xml
<!--  Versione 1.2.0  -->
<Border
    Margin="0,0,0,15"
    Padding="15"
    Background="#ECF0F1"
    CornerRadius="5">
    <StackPanel>
        <TextBlock
            FontSize="16"
            FontWeight="SemiBold"
            Text="Versione 1.2.0 (22 Dicembre 2025)" />
        <TextBlock
            Margin="0,10,0,5"
            FontWeight="SemiBold"
            Text="🆕 Nuove funzionalità:" />
        <TextBlock Margin="10,3" Text="• Sistema di controllo automatico aggiornamenti via HTTPS" />
        <TextBlock Margin="10,3" Text="• Notifiche per nuove versioni disponibili" />
        <TextBlock Margin="10,3" Text="• Verifica integrità file con hash SHA256" />
        <TextBlock
            Margin="0,10,0,5"
            FontWeight="SemiBold"
            Text="🔧 Miglioramenti:" />
        <TextBlock Margin="10,3" Text="• Ottimizzazione prestazioni database" />
        <TextBlock Margin="10,3" Text="• Miglioramenti interfaccia utente" />
    </StackPanel>
</Border>
```

#### 4b. Cambia Colore Blocco Precedente

Cambia il vecchio blocco da `Background="#ECF0F1"` a `Background="#F8F9FA"`:

```xml
<!--  Versione 1.1.2  -->
<Border
    Margin="0,0,0,15"
    Padding="15"
    Background="#F8F9FA"    <!-- Cambiato da #ECF0F1 a #F8F9FA -->
    CornerRadius="5">
```

### 5. ✅ ReleaseNotes.txt - Note Installer
**Percorso**: `docs/ReleaseNotes.txt`

Sovrascrivi TUTTO il contenuto con le note della nuova versione:

```text
═══════════════════════════════════════════════════════════════
  TaoGEST - Gestione Terapia Anticoagulante Orale
  Note di Rilascio - Versione 1.2.0
═══════════════════════════════════════════════════════════════

Data Rilascio: 22 Dicembre 2025

NUOVE FUNZIONALITÀ
─────────────────────────────────────────────────────────────

🆕 Sistema Aggiornamenti Automatici
  • Controllo automatico aggiornamenti via HTTPS
  • Notifiche per nuove versioni disponibili
  • Download diretto installer dalla finestra di notifica
  • Verifica integrità file con hash SHA256

MIGLIORAMENTI
─────────────────────────────────────────────────────────────

⚙️ Prestazioni e Stabilità
  • Ottimizzazione prestazioni database
  • Miglioramenti interfaccia utente

REQUISITI DI SISTEMA
─────────────────────────────────────────────────────────────

• Windows 10 (versione 1809 o successiva) o Windows 11
• .NET 8.0 Desktop Runtime (x64)
• 200 MB spazio su disco
• 4 GB RAM (consigliati)

INSTALLAZIONE
─────────────────────────────────────────────────────────────

1. Eseguire TaoGEST-Setup-v1.2.0.exe
2. Seguire le istruzioni dell'installazione guidata

═══════════════════════════════════════════════════════════════
© 2025 - dr Dario Giorgio Zani
═══════════════════════════════════════════════════════════════
```

### 6. ✅ CHANGELOG.md - Storico Versioni
**Percorso**: `CHANGELOG.md`

Aggiungi DOPO `## [Unreleased]` o all'inizio della lista:

```markdown
## [1.2.0] - 2025-12-22

### Added
- Sistema di controllo automatico aggiornamenti via HTTPS
- Notifiche per nuove versioni con dettagli release
- Download diretto installer dalla finestra di notifica
- Controllo periodico in background ogni 24 ore
- Verifica integrità file con hash SHA256

### Changed
- Migrato sistema aggiornamenti da FTP a HTTPS pubblico
- Ottimizzate prestazioni database
- Miglioramenti interfaccia utente

### Security
- Sistema aggiornamenti basato su HTTPS (no credenziali)
- Verifica hash SHA256 per integrità download
```

## 🔄 Procedura Completa Rilascio

### Step-by-Step

1. **Aggiorna tutti i 6 file sopra elencati**
   - Usa un editor di testo o IDE
   - Sostituisci tutte le occorrenze della vecchia versione

2. **Verifica modifiche**
   ```bash
   git diff
   ```

3. **Commit modifiche versione**
   ```bash
   git add Version.props installer/TaoGEST-Setup.iss src/WarfarinManager.UI/MainWindow.xaml src/WarfarinManager.UI/Views/Dialogs/AboutDialog.xaml docs/ReleaseNotes.txt CHANGELOG.md

   git commit -m "Release: Versione 1.2.0 - [Descrizione]"
   ```

4. **Compila e crea installer**
   ```bash
   dotnet clean
   dotnet build -c Release
   dotnet publish src/WarfarinManager.UI/WarfarinManager.UI.csproj -c Release -r win-x64 --self-contained true
   "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer\TaoGEST-Setup.iss
   ```

5. **Calcola hash e aggiorna version.json**
   ```powershell
   .\scripts\Calculate-InstallerHash.ps1 -UpdateVersionJson
   ```

6. **Push su GitHub**
   ```bash
   git push origin master
   ```

7. **Crea Release su GitHub**
   ```bash
   gh release create v1.2.0 --title "TaoGEST v1.2.0 - [Titolo]" --notes "[Note]" publish/TaoGEST-Setup-v1.2.0.exe
   ```

## 📍 Dove Trovo Questi File Nell'IDE

### Visual Studio / VS Code

1. **Version.props**: Root del progetto
2. **TaoGEST-Setup.iss**: `installer/` folder
3. **MainWindow.xaml**: `src/WarfarinManager.UI/`
4. **AboutDialog.xaml**: `src/WarfarinManager.UI/Views/Dialogs/`
5. **ReleaseNotes.txt**: `docs/`
6. **CHANGELOG.md**: Root del progetto

## ⚠️ Cose Importanti da Ricordare

1. **Consistenza Versioni**: Tutti i file devono avere la STESSA versione

2. **Formato Versione**:
   - `Version.props`: `1.2.0.0` (4 numeri)
   - Tutti gli altri: `1.2.0` (3 numeri)

3. **Colore Changelog in AboutDialog**:
   - Versione CORRENTE: `#ECF0F1` (grigio chiaro)
   - Versioni PRECEDENTI: `#F8F9FA` (grigio più chiaro)

4. **Data Rilascio**: Usa formato italiano "22 Dicembre 2025"

5. **Note di Rilascio**: Sii specifico e utile per l'utente finale

## 🚀 Script Automatico (Futuro)

**TODO**: Creare script PowerShell che faccia tutto automaticamente:

```powershell
.\Update-All-Version.ps1 -Version "1.2.0" -ReleaseDate "22 Dicembre 2025"
```

Lo script dovrebbe:
- Aggiornare tutti i 6 file automaticamente
- Validare che le modifiche siano corrette
- Creare un commit Git con le modifiche
- Mostrare un diff delle modifiche

## 📚 Risorse

- [Semantic Versioning](https://semver.org/)
- [Keep a Changelog](https://keepachangelog.com/)
- [GUIDA-RELEASE-GITHUB.md](GUIDA-RELEASE-GITHUB.md) - Procedura completa release

---

**Ultima modifica**: 2025-12-22
**Versione guida**: 1.0
