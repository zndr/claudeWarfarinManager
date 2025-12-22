# Script per verificare lo stato delle migration nel database SQLite

$dbPath = "$env:LOCALAPPDATA\WarfarinManager\warfarin.db"

Write-Host "Database path: $dbPath" -ForegroundColor Cyan

if (-Not (Test-Path $dbPath)) {
    Write-Host "Database non trovato!" -ForegroundColor Red
    exit 1
}

Write-Host "`nVerifica struttura tabella Patients..." -ForegroundColor Yellow

# Query per vedere le colonne della tabella Patients
$sql = "PRAGMA table_info(Patients);"

Write-Host "`nEsegui questo comando per verificare le colonne:" -ForegroundColor Green
Write-Host "sqlite3 `"$dbPath`" `"$sql`"" -ForegroundColor White

Write-Host "`nPer applicare manualmente la migration:" -ForegroundColor Yellow
Write-Host "1. Installa DB Browser for SQLite: https://sqlitebrowser.org/dl/" -ForegroundColor White
Write-Host "2. Apri: $dbPath" -ForegroundColor White
Write-Host "3. Vai su 'Execute SQL' e esegui:" -ForegroundColor White
Write-Host @"

-- Aggiungi la colonna se non esiste
ALTER TABLE Patients ADD COLUMN IsInitialWizardCompleted INTEGER NOT NULL DEFAULT 0;

-- Marca tutti i pazienti esistenti come 'wizard completato'
-- (così non gli viene richiesto di rifare la configurazione)
UPDATE Patients SET IsInitialWizardCompleted = 1;

-- Verifica
SELECT Id, FirstName, LastName, IsNaive, IsInitialWizardCompleted FROM Patients;

"@ -ForegroundColor Green

Write-Host "`n4. Clicca 'Write Changes' (icona dischetto) per salvare" -ForegroundColor White
Write-Host "5. Riavvia l'applicazione TaoGEST" -ForegroundColor White
