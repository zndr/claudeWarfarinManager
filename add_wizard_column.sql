-- Script SQL per aggiungere la colonna IsInitialWizardCompleted alla tabella Patients
-- Eseguire questo script se il database esiste già

ALTER TABLE Patients ADD COLUMN IsInitialWizardCompleted INTEGER NOT NULL DEFAULT 0;

-- Imposta il flag a TRUE per tutti i pazienti esistenti (considerati già configurati)
UPDATE Patients SET IsInitialWizardCompleted = 1;

-- Verifica
SELECT Id, FirstName, LastName, IsNaive, IsInitialWizardCompleted FROM Patients;
