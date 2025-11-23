# Integration Tests - Suite Completa

## 📋 Panoramica

Suite completa di integration tests per validare il layer Data di WarfarinManager.

## 🎯 Test Coverage

### 1. DatabaseCreationAndSeedingTests
- ✅ Creazione database con tutte le tabelle
- ✅ Seeding InteractionDrugs (20+ farmaci)
- ✅ Seeding IndicationTypes (10+ indicazioni)
- ✅ Validazione indici database
- ✅ Validazione constraints (unique, FK)
- ✅ Conversioni enum → string
- ✅ Timestamp automatici (CreatedAt/UpdatedAt)

### 2. PatientRepositoryIntegrationTests
- ✅ CRUD operations complete
- ✅ Search by FiscalCode
- ✅ Query pazienti con indicazioni attive
- ✅ Query pazienti con controlli INR recenti
- ✅ Full-text search (multi-criteria)
- ✅ Flag metabolizzatore lento
- ✅ Cascade delete

### 3. INRControlRepositoryIntegrationTests
- ✅ Inserimento controlli con dosi giornaliere
- ✅ Storico INR ordinato
- ✅ Filtro per range date
- ✅ **Calcolo TTR da database reale**
- ✅ Identificazione controlli fuori range
- ✅ Query complesse multi-paziente
- ✅ Creazione storico stabile/instabile/variabile

### 4. UnitOfWorkIntegrationTests
- ✅ Accesso a tutti i repository
- ✅ SaveChanges atomico
- ✅ Transazioni con commit
- ✅ Transazioni con rollback
- ✅ Transazioni complesse multi-entità
- ✅ Gestione concorrenza
- ✅ Dispose corretto

### 5. PerformanceTests
- ✅ Caricamento 500 pazienti <1s (PRD requirement)
- ✅ Caricamento dettagli paziente <500ms (PRD requirement)
- ✅ Search in dataset grande <100ms
- ✅ Storico INR 100+ controlli <200ms
- ✅ Bulk insert efficiente
- ✅ Query complesse ottimizzate
- ✅ Index efficiency (FiscalCode lookup <10ms)
- ✅ Memory usage ragionevole (<50 MB per 500 pazienti)
- ✅ Letture concorrenti scalabili

## 🚀 Esecuzione Tests

### Da Visual Studio
1. Apri Test Explorer (Test → Test Explorer)
2. Click "Run All Tests"
3. Visualizza risultati

### Da Command Line
```bash
cd D:\Claude\winTaoGest
dotnet test --filter "FullyQualifiedName~Integration"
```

### Con Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

## 📊 Metriche Attese

| Metrica | Target | Priorità |
|---------|--------|----------|
| Test Pass Rate | 100% | 🔴 Critico |
| Execution Time (totale) | <30s | 🟡 Importante |
| Database Creation | <2s | 🟡 Importante |
| Performance Tests Pass | 100% | 🟢 Desiderabile |

## ⚠️ Note Importanti

### SQLite In-Memory
I test usano SQLite in-memory per:
- ✅ Velocità esecuzione
- ✅ Isolamento totale
- ✅ Schema identico a produzione
- ✅ Nessuna pulizia necessaria

### Dati di Seeding
Tutti i test verificano:
- 20 farmaci con interazioni critiche (FCSA-SIMG validated)
- 10+ indicazioni terapeutiche con target INR corretti
- Validazione contro PRD Sezione 3.3 e 3.2

### Performance Benchmarks
I test di performance validano:
- PRD Section 4.1: Performance requirements
- Caricamento lista: <1s per 500 pazienti
- Dettagli paziente: <500ms
- Calcolo TTR: <200ms

## 🔧 Troubleshooting

### Test Falliscono su Timestamps
**Problema**: Differenze UTC/Local time  
**Soluzione**: I test usano `BeCloseTo()` con tolleranza 1 secondo

### Test Falliscono su Concurrency
**Problema**: SQLite in-memory non supporta vera concorrenza  
**Soluzione**: Test usa context separati per simulare

### Test Lenti
**Problema**: Troppi dati generati  
**Soluzione**: Riduci count in PerformanceTests (configurable)

## 📝 Next Steps

Dopo validazione tests:
1. ✅ Tutti i test passano → Procedi a UI layer
2. ⚠️ Alcuni falliscono → Debug e fix
3. 📈 Performance issues → Ottimizza query/indici

## 🎓 Best Practices Applicati

- ✅ Arrange-Act-Assert pattern
- ✅ Descriptive test names
- ✅ FluentAssertions per readability
- ✅ IDisposable per cleanup
- ✅ Test isolation (ogni test indipendente)
- ✅ Factory pattern per DbContext
- ✅ Mock data generation realistico
- ✅ Performance benchmarks del PRD
