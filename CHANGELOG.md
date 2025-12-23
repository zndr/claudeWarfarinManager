# Changelog

Tutte le modifiche importanti a questo progetto saranno documentate in questo file.

Il formato è basato su [Keep a Changelog](https://keepachangelog.com/it/1.0.0/),
e questo progetto aderisce al [Semantic Versioning](https://semver.org/lang/it/).

## [1.2.3.1] - 2025-12-23

### Fixed

- **HOTFIX CRITICO**: Installer v1.2.3 richiedeva .NET 8 anche se già installato
- Rimosso controllo errato .NET Runtime dall'installer (chiave registro sbagliata)
- L'applicazione è self-contained e non richiede .NET installato separatamente
- Aggiornati requisiti di sistema nelle note di rilascio (rimosso requisito .NET)

## [1.2.3] - 2025-12-22

### Fixed
- Risolto errore "SQLite error: no such column certaintyLevel" durante eliminazione pazienti su database legacy (v1.0.0)
- Correzione automatica dello schema database all'avvio per database provenienti da versioni precedenti

### Changed
- Finestra Controllo INR ora si adatta automaticamente agli schermi piccoli (es. laptop 15")
- Finestra Modifica INR ora ridimensionabile e adattativa
- Aggiunto MinWidth/MinHeight e MaxWidth/MaxHeight vincolati all'area di lavoro
- Le finestre INR si centrano correttamente anche su schermi con risoluzioni inferiori a 1400x950

## [1.2.2.2] - 2025-12-22

### Fixed
- Corretto bug controllo aggiornamenti che mostrava notifica anche con versione già aggiornata
- UpdateNotificationService ora usa GetEntryAssembly() invece di GetExecutingAssembly()
- AboutDialog mostra versione completa a 4 componenti (es. 1.2.2.2)

### Added
- Logging dettagliato per diagnostica confronto versioni in UpdateCheckerService
- Checklist release in CLAUDE.md per evitare dimenticanze

## [1.2.2.1] - 2025-12-22

### Changed
- Controllo aggiornamenti ora completamente silenzioso all'avvio
- Notifiche mostrate SOLO quando è disponibile una nuova versione
- Controllo manuale dal menu mantiene feedback visibile
- Migliorata esperienza utente eliminando messaggi superflui

### Fixed
- UpdateNotificationService: parametro showNoUpdateMessage=false per controlli automatici
- UpdateNotificationService: parametro showNoUpdateMessage=true solo per controlli manuali
- Eliminati messaggi "già aggiornato" durante avvio e controlli periodici

## [1.2.2] - 2025-12-22

### Fixed
- Risolto problema critico: sistema aggiornamenti non rilevava nuove versioni
- Configurato correttamente URL HTTPS in appsettings.Production.json
- Aggiornato valore di default hardcoded in App.xaml.cs per URL version.json

### Changed
- Aggiunto appsettings.Production.json con configurazione production-ready
- URL configurato: https://raw.githubusercontent.com/zndr/claudeWarfarinManager/master/version.json

## [1.2.1] - 2025-12-22

### Added
- GUIDA-AGGIORNAMENTO-VERSIONE.md: Guida completa per aggiornare tutti i riferimenti di versione
- GUIDA-RELEASE-GITHUB.md: Procedura dettagliata per creare release su GitHub
- Script Update-Version.ps1 migliorato per aggiornamenti centralizzati

### Changed
- Migliorata documentazione sistema aggiornamenti HTTPS
- Aggiornati tutti i riferimenti di versione in modo consistente
- AboutDialog.xaml ora mostra changelog versione 1.2.1

## [1.2.0] - 2025-12-22

### Added
- Sistema di controllo automatico aggiornamenti via HTTPS
- Notifiche per nuove versioni disponibili con dettagli release
- Download diretto installer dalla finestra di notifica
- Controllo periodico in background ogni 24 ore
- Verifica integrità file con hash SHA256
- AGGIORNAMENTI-HTTPS.md: Documentazione completa sistema aggiornamenti
- GUIDA-RAPIDA-HASH.md: Guida per calcolo hash SHA256

### Changed
- Migrato sistema aggiornamenti da FTP a HTTPS pubblico
- Ottimizzate prestazioni database
- Miglioramenti interfaccia utente

### Security
- Sistema aggiornamenti basato su HTTPS pubblico (no credenziali)
- Verifica hash SHA256 per integrità download
- Protezione contro file modificati o corrotti

## [1.1.2] - 2025-12-21

### Added
- Icona applicazione nella barra delle applicazioni e nel menu di sistema
- File CHANGELOG.md per tracciare le versioni

### Changed
- Aggiornato numero di versione a 1.1.2
- Aggiornato testo versione nella status bar dell'applicazione

## [1.1.1] - 2025-12-21

### Added
- Wizard di configurazione iniziale paziente con step guidati:
  - Dati anagrafici e clinici di base
  - Calcolo score CHA2DS2-VASc per rischio trombotico
  - Calcolo score HAS-BLED per rischio emorragico
  - Valutazione controindicazioni pre-TAO
- Nuovi campi nella tabella Patient per gestire il completamento del wizard
- Migration database per aggiungere campo IsInitialWizardCompleted

### Changed
- Modificata logica di creazione nuovo paziente per includere wizard iniziale
- Refactoring dei ViewModel per separare wizard da form classico

## [1.1.0] - 2025-12-20

### Added
- Funzionalità di gestione terapia anticoagulante orale
- Database SQLite con Entity Framework Core
- Interfaccia WPF per gestione pazienti
- Calcolo dosaggio Warfarin basato su INR
- Grafici trend INR con LiveCharts2
- Export PDF con QuestPDF
- Guide professionali integrate (HTML e PDF)

### Changed
- Migliorata architettura MVVM con CommunityToolkit.Mvvm
- Ottimizzato sistema di logging con Serilog

## [1.0.0] - 2025-12-15

### Added
- Release iniziale
- Gestione pazienti in terapia con Warfarin
- Calcolo score CHA2DS2-VASc e HAS-BLED
- Verifica interazioni farmacologiche
- Gestione bridge therapy
- Switch terapia Warfarin ↔ DOAC
