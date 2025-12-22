# Guida Completa al Processo di Release

Questa guida documenta TUTTI i passaggi necessari per creare una nuova release di TaoGEST in modo completo e consistente.

## Prerequisiti

- PowerShell 5.1 o superiore
- .NET 8.0 SDK
- Inno Setup 6 installato in `C:\Program Files (x86)\Inno Setup 6\`
- GitHub CLI (`gh`) configurato e autenticato
- Repository locale aggiornato (`git pull`)

## Processo Completo di Release

### 1. Aggiornamento Numero di Versione

**Script automatico** (raccomandato):
```powershell
.\Update-Version.ps1 -NewVersion "X.Y.Z.W"
```

Questo aggiorna automaticamente:
- `Version.props` (AssemblyVersion, FileVersion, VersionPrefix)
- `installer/TaoGEST-Setup.iss` (MyAppVersion)
- `src/WarfarinManager.UI/MainWindow.xaml` (testo versione in basso a sinistra)

**IMPORTANTE**: Lo script Update-Version.ps1 usa solo i primi 3 componenti (X.Y.Z), quindi per versioni come 1.2.2.1 è necessario aggiornare manualmente l'installer:

```powershell
# Modifica manualmente installer/TaoGEST-Setup.iss
#define MyAppVersion "1.2.2.1"  # <- aggiorna qui
```

### 2. Aggiornamento Changelog e Documentazione

#### a. MainWindow.xaml
Aggiorna il testo della versione nell'angolo in basso a sinistra:
```xml
<TextBlock Text="Versione X.Y.Z.W" />
```

**File**: `src/WarfarinManager.UI/MainWindow.xaml` (cerca "Versione")

#### b. AboutDialog.xaml
Aggiorna il tab "Novità" aggiungendo la nuova versione IN CIMA:

```xml
<!--  Versione X.Y.Z.W  -->
<Border
    Margin="0,0,0,15"
    Padding="15"
    Background="#ECF0F1"    <!-- Versione corrente: sfondo più scuro -->
    CornerRadius="5">
    <StackPanel>
        <TextBlock
            FontSize="16"
            FontWeight="SemiBold"
            Text="Versione X.Y.Z.W (GG Mese AAAA)" />
        <TextBlock
            Margin="0,10,0,5"
            FontWeight="SemiBold"
            Foreground="#27AE60"    <!-- Verde per migliorie -->
            Text="Miglioramenti:" />
        <TextBlock Margin="10,3" Text="• Prima miglioria" />
        <!-- ... -->
    </StackPanel>
</Border>
```

**Note sui colori categorie**:
- `#27AE60` (verde): Miglioramenti/Features
- `#E74C3C` (rosso): Correzioni critiche
- `#3498DB` (blu): Nuove funzionalità

**File**: `src/WarfarinManager.UI/Views/Dialogs/AboutDialog.xaml`

#### c. CHANGELOG.md
Aggiungi la nuova versione IN CIMA seguendo lo standard Keep a Changelog:

```markdown
## [X.Y.Z.W] - AAAA-MM-GG

### Added
- Nuove funzionalità aggiunte

### Changed
- Modifiche a funzionalità esistenti

### Fixed
- Bug corretti

### Security
- Problemi di sicurezza risolti
```

**File**: `CHANGELOG.md`

#### d. docs/ReleaseNotes.txt
Aggiorna le note di rilascio per l'installer Inno Setup:

```
═══════════════════════════════════════════════════════════════
  TaoGEST - Gestione Terapia Anticoagulante Orale
  Note di Rilascio - Versione X.Y.Z.W
═══════════════════════════════════════════════════════════════

Data Rilascio: GG Mese AAAA

SEZIONE PRINCIPALE
─────────────────────────────────────────────────────────────

✨ Titolo Sezione
  • Punto elenco 1
  • Punto elenco 2

INSTALLAZIONE
─────────────────────────────────────────────────────────────

1. Eseguire TaoGEST-Setup-vX.Y.Z.W.exe
2. Seguire le istruzioni dell'installazione guidata
...
```

**File**: `docs/ReleaseNotes.txt`

### 3. Build e Publish

```powershell
# Pulizia e restore
dotnet clean
dotnet restore

# Publish Release
dotnet publish src/WarfarinManager.UI/WarfarinManager.UI.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -o publish
```

**Verifica**: Controlla che non ci siano errori di compilazione. Gli warning sono accettabili.

### 4. Creazione Installer

```powershell
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "installer\TaoGEST-Setup.iss"
```

**Output**: `publish/TaoGEST-Setup-vX.Y.Z.W.exe`

**Verifica dimensione installer**:
```powershell
Get-ChildItem -Path publish -Filter 'TaoGEST-Setup-v*.exe' | Select-Object Name, Length, LastWriteTime
```

### 5. Calcolo Hash SHA256

```powershell
Get-FileHash -Path "publish/TaoGEST-Setup-vX.Y.Z.W.exe" -Algorithm SHA256 | Select-Object Hash
```

**Copia l'hash** (tutto in minuscolo per consistenza).

### 6. Aggiornamento version.json

Modifica il file `version.json` nella root del progetto:

```json
{
  "Version": "X.Y.Z.W",
  "DownloadUrl": "https://github.com/zndr/claudeWarfarinManager/releases/download/vX.Y.Z.W/TaoGEST-Setup-vX.Y.Z.W.exe",
  "ReleaseDate": "AAAA-MM-GGTHH:MM:SS",
  "IsCritical": false,  // true solo per hotfix critici
  "FileSize": 12345678,  // dimensione in bytes
  "Sha256Hash": "hash-calcolato-sopra",
  "ReleaseNotes": "Release notes in formato testo"
}
```

**Note**:
- L'hash deve essere in minuscolo
- FileSize è in bytes (usa `(Get-Item "publish/TaoGEST-Setup-vX.Y.Z.W.exe").Length`)
- IsCritical=true mostra notifica urgente agli utenti

### 7. Commit e Push

```bash
git add -A
git commit -m "Release: Versione X.Y.Z.W - Titolo Release

DESCRIZIONE MODIFICHE:
• Modifica 1
• Modifica 2

FILE MODIFICATI:
• file1
• file2

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude Sonnet 4.5 <noreply@anthropic.com>"

git push
```

### 8. Creazione Release GitHub

```bash
gh release create vX.Y.Z.W \
    "publish/TaoGEST-Setup-vX.Y.Z.W.exe" \
    --title "TaoGEST vX.Y.Z.W - Titolo Release" \
    --notes "$(cat <<'EOF'
✨ **TaoGEST vX.Y.Z.W** - Titolo Release

## 🎯 Migliorie Implementate

- Prima miglioria
- Seconda miglioria

## 📋 Dettagli Tecnici

- Dettaglio tecnico 1
- Dettaglio tecnico 2

## 📥 Installazione

1. **Chiudi TaoGEST** se aperto
2. **Scarica** l'installer qui sotto
3. **Esegui** l'installer
4. **Riavvia** l'applicazione

## 🔐 Verifica Integrità

**SHA256**: `hash-in-minuscolo`

Per verificare l'integrità del file scaricato:
\`\`\`powershell
Get-FileHash -Path "TaoGEST-Setup-vX.Y.Z.W.exe" -Algorithm SHA256
\`\`\`

---

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)"
```

### 9. Verifica Release

```bash
# Verifica release creata
gh release view vX.Y.Z.W

# Testa download manualmente
# Naviga su: https://github.com/zndr/claudeWarfarinManager/releases/tag/vX.Y.Z.W
```

**Checklist finale**:
- [ ] Installer è caricato su GitHub
- [ ] Hash SHA256 corrisponde
- [ ] version.json è aggiornato su master
- [ ] Repository è pubblico (per permettere download version.json)
- [ ] Download URL è corretto nella release

### 10. Test Aggiornamento

1. Installa una versione precedente (es. 1.2.0)
2. Avvia l'applicazione
3. Aspetta qualche minuto (cache GitHub potrebbe ritardare)
4. Verifica che compaia la notifica di aggiornamento
5. Testa il download tramite la notifica
6. Installa l'aggiornamento
7. Verifica versione in AboutDialog

## Troubleshooting

### L'app non rileva l'aggiornamento

**Causa**: Cache di GitHub per raw.githubusercontent.com

**Soluzione**:
- Aspetta 5-10 minuti per invalidazione cache
- Verifica che il repository sia pubblico
- Controlla manualmente: `https://raw.githubusercontent.com/zndr/claudeWarfarinManager/master/version.json`

### Hash SHA256 non corrisponde

**Causa**: Installer rigenerato dopo calcolo hash

**Soluzione**:
1. Ricalcola hash dopo generazione finale installer
2. Aggiorna version.json
3. Commit e push
4. Aggiorna release notes con `gh release edit vX.Y.Z.W --notes "..."`

### AboutDialog mostra versione vecchia

**Causa**: Dimenticato aggiornamento AboutDialog.xaml

**Soluzione**:
1. Aggiorna AboutDialog.xaml
2. Rebuild progetto
3. Rigenera installer
4. Aggiorna release su GitHub con `gh release upload vX.Y.Z.W "publish/..." --clobber`

## Note Importanti

1. **Ordine delle operazioni**: Segui ESATTAMENTE l'ordine indicato. In particolare:
   - Changelog PRIMA del build
   - Build PRIMA del calcolo hash
   - version.json PRIMA del push
   - Push PRIMA della release GitHub

2. **Consistenza versioni**: La versione deve essere identica in:
   - Version.props
   - installer/TaoGEST-Setup.iss
   - MainWindow.xaml
   - AboutDialog.xaml
   - CHANGELOG.md
   - docs/ReleaseNotes.txt
   - version.json
   - Tag GitHub (vX.Y.Z.W)

3. **Repository pubblico**: Il repository DEVE essere pubblico per permettere il download di version.json da parte degli utenti.

4. **Formato emoji**: Usa emoji consistenti:
   - ✨ Feature/Miglioria
   - 🐛 Bug fix
   - 🔧 Manutenzione
   - 📝 Documentazione
   - 🎯 Obiettivo/Focus
   - 📋 Dettagli
   - 🔒 Sicurezza
   - 📥 Download/Installazione
   - 🔐 Hash/Verifica

5. **Semantic Versioning**:
   - X.Y.Z.W
   - X = Major (breaking changes)
   - Y = Minor (nuove feature compatibili)
   - Z = Patch (bug fix)
   - W = Hotfix/Build (fix minori, aggiornamenti UI/docs)

## Checklist Finale Release

Prima di considerare la release completa:

- [ ] Tutti i file di versione aggiornati
- [ ] Changelog completo e dettagliato
- [ ] Build completata senza errori
- [ ] Installer generato correttamente
- [ ] Hash SHA256 calcolato e inserito
- [ ] version.json aggiornato
- [ ] Commit pushato su master
- [ ] Release GitHub creata
- [ ] Installer caricato su GitHub
- [ ] AboutDialog mostra versione corretta
- [ ] Test aggiornamento da versione precedente funziona

## Script Rapido

Per velocizzare il processo, puoi creare uno script PowerShell:

```powershell
# ReleaseComplete.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$Version
)

# 1. Update version
.\Update-Version.ps1 -NewVersion $Version

# 2. Build
dotnet clean
dotnet publish src/WarfarinManager.UI/WarfarinManager.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -o publish

# 3. Create installer
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "installer\TaoGEST-Setup.iss"

# 4. Calculate hash
$hash = (Get-FileHash -Path "publish/TaoGEST-Setup-v$Version.exe" -Algorithm SHA256).Hash.ToLower()
Write-Host "SHA256: $hash" -ForegroundColor Green

# 5. Reminder
Write-Host "`nProssimi passaggi manuali:" -ForegroundColor Yellow
Write-Host "1. Aggiorna version.json con hash: $hash"
Write-Host "2. Aggiorna MainWindow.xaml, AboutDialog.xaml, CHANGELOG.md, ReleaseNotes.txt"
Write-Host "3. git add -A && git commit && git push"
Write-Host "4. gh release create v$Version publish/TaoGEST-Setup-v$Version.exe ..."
```

---

**Ultimo aggiornamento**: 22 Dicembre 2025
**Versione guida**: 1.0
