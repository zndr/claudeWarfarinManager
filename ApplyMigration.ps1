# Script PowerShell per applicare manualmente la migration al database SQLite

$dbPath = "$env:LOCALAPPDATA\WarfarinManager\warfarin.db"

Write-Host "Database path: $dbPath"

if (-Not (Test-Path $dbPath)) {
    Write-Host "Database non trovato. Verrà creato automaticamente al primo avvio dell'applicazione." -ForegroundColor Yellow
    Write-Host "Avvia l'applicazione per creare il database con tutte le migration applicate." -ForegroundColor Green
    exit 0
}

Write-Host "Database trovato. Controllo se la colonna IsInitialWizardCompleted esiste..." -ForegroundColor Cyan

# Usa sqlite3 per verificare e aggiungere la colonna
# Prima verifica se sqlite3 è disponibile
$sqlite3Path = "sqlite3.exe"
if (-Not (Get-Command $sqlite3Path -ErrorAction SilentlyContinue)) {
    Write-Host ""
    Write-Host "ISTRUZIONI MANUALI:" -ForegroundColor Yellow
    Write-Host "1. Chiudi l'applicazione TaoGEST se è aperta" -ForegroundColor White
    Write-Host "2. Scarica DB Browser for SQLite da: https://sqlitebrowser.org/dl/" -ForegroundColor White
    Write-Host "3. Apri il database: $dbPath" -ForegroundColor White
    Write-Host "4. Vai su 'Execute SQL' e esegui:" -ForegroundColor White
    Write-Host ""
    Write-Host "   ALTER TABLE Patients ADD COLUMN IsInitialWizardCompleted INTEGER NOT NULL DEFAULT 0;" -ForegroundColor Green
    Write-Host "   UPDATE Patients SET IsInitialWizardCompleted = 1;" -ForegroundColor Green
    Write-Host ""
    Write-Host "5. Salva le modifiche e chiudi DB Browser" -ForegroundColor White
    Write-Host "6. Riavvia TaoGEST" -ForegroundColor White
    Write-Host ""
    Write-Host "OPPURE:" -ForegroundColor Yellow
    Write-Host "Elimina il database a: $dbPath" -ForegroundColor White
    Write-Host "Il database verrà ricreato automaticamente al prossimo avvio con tutte le migration applicate." -ForegroundColor White
    exit 1
}

# Se sqlite3 è disponibile, applica la migration
Write-Host "Tentativo di applicare la migration..." -ForegroundColor Cyan

$sql = @"
-- Controlla se la colonna esiste già
SELECT name FROM pragma_table_info('Patients') WHERE name='IsInitialWizardCompleted';
"@

$result = $sql | & $sqlite3Path $dbPath

if ($result -eq "IsInitialWizardCompleted") {
    Write-Host "La colonna IsInitialWizardCompleted esiste già!" -ForegroundColor Green
    exit 0
}

# Applica la migration
$migrationSql = @"
ALTER TABLE Patients ADD COLUMN IsInitialWizardCompleted INTEGER NOT NULL DEFAULT 0;
UPDATE Patients SET IsInitialWizardCompleted = 1;
"@

$migrationSql | & $sqlite3Path $dbPath

if ($LASTEXITCODE -eq 0) {
    Write-Host "Migration applicata con successo!" -ForegroundColor Green
    Write-Host "Tutti i pazienti esistenti sono stati marcati come 'wizard completato'" -ForegroundColor Cyan
} else {
    Write-Host "Errore durante l'applicazione della migration" -ForegroundColor Red
    exit 1
}
