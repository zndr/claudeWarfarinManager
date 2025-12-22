# 📜 Scripts TaoGEST

Raccolta di script PowerShell per automatizzare operazioni comuni nello sviluppo di TaoGEST.

## 🔢 Update-Version.ps1

Aggiorna la versione del progetto in modo centralizzato.

```powershell
.\Update-Version.ps1 -NewVersion "1.2.0.0"
```

---

## 🔐 Calculate-InstallerHash.ps1

Calcola l'hash SHA256 dell'installer per verifica integrità.

```powershell
# Calcola hash e aggiorna automaticamente version.json
.\Calculate-InstallerHash.ps1 -UpdateVersionJson
```

Vedi [GUIDA-RAPIDA-HASH.md](../GUIDA-RAPIDA-HASH.md) e [AGGIORNAMENTI-HTTPS.md](../AGGIORNAMENTI-HTTPS.md) per dettagli completi.

---

## 📚 sync-guides.ps1

Sincronizza le guide HTML/PDF dalla cartella `docs` alla cartella `Resources\Guides` del progetto.

## 📋 File Sincronizzati

Lo script sincronizza automaticamente i seguenti file:

| File Sorgente (docs) | File Destinazione (Resources/Guides) |
|---------------------|--------------------------------------|
| `interactions.html` | `interactions.html` |
| `LineeGuida.html` | `linee-guida-tao.html` |
| `LineeGuida.pdf` | `LineeGuida.pdf` |
| `Guida Warfarin per pazienti.pdf` | `Guida Warfarin per pazienti.pdf` |
| `Algoritmo Gestione INR.html` | `algoritmo-gestione-inr.html` |
| `infografica-tao.html` | `infografica-tao.html` |

## 🔄 Sincronizzazione Automatica

La sincronizzazione avviene **automaticamente** ad ogni build del progetto grazie a un target MSBuild configurato nel file `.csproj`.

### Come funziona:

1. Modifichi un file nella cartella `D:\Claude\winTaoGest\docs`
2. Esegui **Build** o **Rebuild** in Visual Studio
3. Lo script PowerShell viene eseguito automaticamente prima della compilazione
4. I file modificati vengono copiati in `Resources\Guides`
5. Il build continua normalmente e copia i file nella cartella di output

## 🛠️ Sincronizzazione Manuale

Se preferisci eseguire la sincronizzazione manualmente (senza fare il build):

### Opzione 1: Script PowerShell (Raccomandato)

```powershell
cd D:\Claude\winTaoGest\scripts
powershell -ExecutionPolicy Bypass -File sync-guides.ps1
```

**Vantaggi:**
- Mostra informazioni dettagliate su ogni file
- Copia solo i file modificati (più veloce)
- Colorato e facile da leggere

### Opzione 2: Script Batch (Più Semplice)

Doppio click su `sync-guides.bat` oppure:

```cmd
cd D:\Claude\winTaoGest\scripts
sync-guides.bat
```

**Vantaggi:**
- Non richiede PowerShell
- Esecuzione immediata con doppio click
- Più semplice per utenti meno esperti

## 📝 Workflow Consigliato

### Scenario 1: Modifiche Occasionali
1. Modifica i file in `D:\Claude\winTaoGest\docs`
2. Salva le modifiche
3. Esegui **Build** o **Run** in Visual Studio
4. Le modifiche saranno automaticamente sincronizzate e visibili nell'app

### Scenario 2: Modifiche Frequenti (Sviluppo Attivo)
1. Modifica i file in `D:\Claude\winTaoGest\docs`
2. Salva le modifiche
3. Esegui `sync-guides.bat` manualmente
4. Esegui **Run** (senza rebuild) in Visual Studio per vedere le modifiche più velocemente

## ⚙️ Configurazione

### Disabilitare la Sincronizzazione Automatica

Se vuoi disabilitare la sincronizzazione automatica durante il build, commenta il target nel file `.csproj`:

```xml
<!-- Pre-build: Sincronizza guide da docs a Resources/Guides -->
<!--
<Target Name="SyncGuidesFromDocs" BeforeTargets="PreBuildEvent">
  <Exec Command="powershell -ExecutionPolicy Bypass -NoProfile -File &quot;$(MSBuildProjectDirectory)\..\..\scripts\sync-guides.ps1&quot;"
        ContinueOnError="true"
        IgnoreExitCode="false" />
</Target>
-->
```

### Personalizzare i Percorsi

Se hai i file in percorsi diversi, puoi modificare le variabili all'inizio degli script:

**PowerShell (`sync-guides.ps1`):**
```powershell
param(
    [string]$SourceDir = "TUO_PERCORSO_DOCS",
    [string]$TargetDir = "TUO_PERCORSO_RESOURCES"
)
```

**Batch (`sync-guides.bat`):**
```batch
set SOURCE_DIR=TUO_PERCORSO_DOCS
set TARGET_DIR=TUO_PERCORSO_RESOURCES
```

## 🐛 Troubleshooting

### Lo script PowerShell non viene eseguito durante il build

**Problema:** Execution Policy di Windows blocca lo script.

**Soluzione:** Apri PowerShell come amministratore ed esegui:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### I file non vengono aggiornati nell'applicazione

**Problema:** Visual Studio potrebbe avere i file in cache.

**Soluzione:**
1. Chiudi l'applicazione se è in esecuzione
2. In Visual Studio: **Build → Clean Solution**
3. Esegui **Build → Rebuild Solution**

### Lo script non trova i file

**Problema:** I percorsi non sono corretti.

**Soluzione:** Verifica che i percorsi in `sync-guides.ps1` o `sync-guides.bat` corrispondano alla tua struttura di directory.

## 📊 Output dello Script

### Esempio di output PowerShell:
```
=== Sincronizzazione Guide TaoGEST ===
Sorgente: D:\Claude\winTaoGest\docs
Destinazione: D:\Claude\winTaoGest\src\WarfarinManager.UI\Resources\Guides

✅ Copiato: interactions.html → interactions.html
⏭️  Già aggiornato: LineeGuida.pdf
✅ Copiato: Algoritmo Gestione INR.html → algoritmo-gestione-inr.html

=== Riepilogo ===
File copiati: 2
File già aggiornati: 4

✓ Sincronizzazione completata! Esegui il rebuild dell'applicazione.
```

## 📌 Note Importanti

- ✅ Lo script copia **solo i file modificati** (controlla le date di modifica)
- ✅ Non cancella mai i file di destinazione
- ✅ Preserva i nomi dei file anche se diversi tra sorgente e destinazione
- ✅ È sicuro eseguirlo più volte
- ✅ Compatibile con Git (i file in `Resources\Guides` sono tracciati)

## 🆘 Supporto

Per problemi o domande:
1. Controlla la sezione Troubleshooting
2. Verifica i percorsi nei file di script
3. Controlla che i file sorgente esistano in `docs`
