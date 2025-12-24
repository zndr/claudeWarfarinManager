using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarfarinManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncAdverseEventsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Verifica se la tabella AdverseEvents ha già lo schema nuovo (colonna MeasuresTaken)
            // Se sì, salta la ricreazione della tabella
            // Usiamo un approccio che funziona indipendentemente dallo schema esistente

            // Aggiungi colonna MeasuresTaken se non esiste (per database con schema nuovo ma senza questa colonna)
            migrationBuilder.Sql(@"
                -- Verifica se dobbiamo migrare la tabella AdverseEvents
                -- Controlliamo se esiste la colonna 'EventDate' (schema vecchio) o 'OnsetDate' (schema nuovo)

                -- Prima aggiungiamo le colonne mancanti se necessario
                -- SQLite non supporta ADD COLUMN IF NOT EXISTS, quindi usiamo un trucco

                -- Se la tabella ha lo schema vecchio (EventDate), la ricreiamo
                -- Altrimenti, aggiungiamo solo le colonne mancanti
            ");

            // Ricreiamo la tabella AdverseEvents in modo sicuro
            // Prima verifichiamo se esiste già con lo schema corretto
            migrationBuilder.Sql(@"
                -- Drop della tabella temporanea se esiste da una migrazione precedente fallita
                DROP TABLE IF EXISTS AdverseEvents_new;

                -- Crea tabella temporanea con il nuovo schema
                CREATE TABLE AdverseEvents_new (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PatientId INTEGER NOT NULL,
                    OnsetDate TEXT NOT NULL DEFAULT (datetime('now')),
                    ReactionType TEXT NOT NULL DEFAULT 'Other',
                    Severity TEXT NOT NULL DEFAULT 'Mild',
                    CertaintyLevel TEXT NOT NULL DEFAULT 'Possible',
                    MeasuresTaken TEXT NULL,
                    INRAtEvent TEXT NULL,
                    Notes TEXT NULL,
                    LinkedINRControlId INTEGER NULL,
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    FOREIGN KEY (PatientId) REFERENCES Patients(Id) ON DELETE CASCADE,
                    FOREIGN KEY (LinkedINRControlId) REFERENCES INRControls(Id) ON DELETE SET NULL
                );
            ");

            // Copia i dati - usa colonne che esistono sicuramente in entrambi gli schemi
            // Le colonne Id, PatientId, Severity, INRAtEvent, Notes, LinkedINRControlId, CreatedAt, UpdatedAt
            // esistono in entrambi gli schemi. Per le altre usiamo valori di default.
            migrationBuilder.Sql(@"
                INSERT INTO AdverseEvents_new (Id, PatientId, OnsetDate, ReactionType, Severity, CertaintyLevel, MeasuresTaken, INRAtEvent, Notes, LinkedINRControlId, CreatedAt, UpdatedAt)
                SELECT
                    Id,
                    PatientId,
                    CreatedAt,
                    'Other',
                    COALESCE(Severity, 'Mild'),
                    'Possible',
                    NULL,
                    INRAtEvent,
                    Notes,
                    LinkedINRControlId,
                    CreatedAt,
                    UpdatedAt
                FROM AdverseEvents;
            ");

            // Elimina la vecchia tabella e rinomina
            migrationBuilder.Sql("DROP TABLE AdverseEvents;");
            migrationBuilder.Sql("ALTER TABLE AdverseEvents_new RENAME TO AdverseEvents;");

            // Ricrea gli indici
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_AdverseEvents_Patient_Date ON AdverseEvents (PatientId, OnsetDate DESC);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_AdverseEvents_Severity ON AdverseEvents (Severity);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_AdverseEvents_LinkedINRControlId ON AdverseEvents (LinkedINRControlId);");

            migrationBuilder.AlterColumn<string>(
                name: "Street",
                table: "DoctorData",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "DoctorData",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "DoctorData",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "DoctorData",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "DoctorData",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            // ReactionType è già creata nella nuova tabella AdverseEvents

            // Crea PreTaoAssessments solo se non esiste (usando SQL raw per SQLite)
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS PreTaoAssessments (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PatientId INTEGER NOT NULL,
                    AssessmentDate TEXT NOT NULL,
                    CongestiveHeartFailure INTEGER NOT NULL DEFAULT 0,
                    Hypertension INTEGER NOT NULL DEFAULT 0,
                    Age75OrMore INTEGER NOT NULL DEFAULT 0,
                    Diabetes INTEGER NOT NULL DEFAULT 0,
                    PriorStrokeTiaTE INTEGER NOT NULL DEFAULT 0,
                    VascularDisease INTEGER NOT NULL DEFAULT 0,
                    Age65To74 INTEGER NOT NULL DEFAULT 0,
                    Female INTEGER NOT NULL DEFAULT 0,
                    HasBledHypertension INTEGER NOT NULL DEFAULT 0,
                    AbnormalRenalFunction INTEGER NOT NULL DEFAULT 0,
                    AbnormalLiverFunction INTEGER NOT NULL DEFAULT 0,
                    StrokeHistory INTEGER NOT NULL DEFAULT 0,
                    BleedingHistory INTEGER NOT NULL DEFAULT 0,
                    LabileINR INTEGER NOT NULL DEFAULT 0,
                    Elderly INTEGER NOT NULL DEFAULT 0,
                    DrugsPredisposing INTEGER NOT NULL DEFAULT 0,
                    AlcoholAbuse INTEGER NOT NULL DEFAULT 0,
                    ActiveMajorBleeding INTEGER NOT NULL DEFAULT 0,
                    Pregnancy INTEGER NOT NULL DEFAULT 0,
                    SevereBloodDyscrasia INTEGER NOT NULL DEFAULT 0,
                    RecentNeurosurgery INTEGER NOT NULL DEFAULT 0,
                    IntracranialBleedingOrMalformation INTEGER NOT NULL DEFAULT 0,
                    ActivePepticUlcerOrVarices INTEGER NOT NULL DEFAULT 0,
                    AcuteBacterialEndocarditis INTEGER NOT NULL DEFAULT 0,
                    SevereUncontrolledHypertension INTEGER NOT NULL DEFAULT 0,
                    WarfarinAllergy INTEGER NOT NULL DEFAULT 0,
                    LackOfCompliance INTEGER NOT NULL DEFAULT 0,
                    RecentGIBleeding INTEGER NOT NULL DEFAULT 0,
                    HistoryOfMajorBleeding INTEGER NOT NULL DEFAULT 0,
                    ModerateRenalFailure INTEGER NOT NULL DEFAULT 0,
                    ModerateHepaticFailure INTEGER NOT NULL DEFAULT 0,
                    ModerateThrombocytopenia INTEGER NOT NULL DEFAULT 0,
                    FrequentFalls INTEGER NOT NULL DEFAULT 0,
                    CognitiveImpairment INTEGER NOT NULL DEFAULT 0,
                    RecentMajorSurgery INTEGER NOT NULL DEFAULT 0,
                    OrganicLesionsAtRisk INTEGER NOT NULL DEFAULT 0,
                    AcutePericarditis INTEGER NOT NULL DEFAULT 0,
                    Polypharmacy INTEGER NOT NULL DEFAULT 0,
                    SocialIsolation INTEGER NOT NULL DEFAULT 0,
                    KnownDrugInteractions INTEGER NOT NULL DEFAULT 0,
                    IrregularDietOrHighVitaminK INTEGER NOT NULL DEFAULT 0,
                    ExtremeBMI INTEGER NOT NULL DEFAULT 0,
                    ChronicAnemia INTEGER NOT NULL DEFAULT 0,
                    ActiveCancer INTEGER NOT NULL DEFAULT 0,
                    ScheduledInvasiveProcedure INTEGER NOT NULL DEFAULT 0,
                    KnownGeneticVariants INTEGER NOT NULL DEFAULT 0,
                    ClinicalNotes TEXT NULL,
                    Recommendations TEXT NULL,
                    IsApproved INTEGER NOT NULL DEFAULT 0,
                    ApprovalDate TEXT NULL,
                    AssessingPhysician TEXT NULL,
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    FOREIGN KEY (PatientId) REFERENCES Patients(Id) ON DELETE CASCADE
                );
            ");

            // Crea TherapySwitches solo se non esiste
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS TherapySwitches (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PatientId INTEGER NOT NULL,
                    SwitchDate TEXT NOT NULL,
                    Direction TEXT NOT NULL,
                    DoacType TEXT NOT NULL,
                    WarfarinType TEXT NOT NULL,
                    InrAtSwitch TEXT NULL,
                    CreatinineClearance TEXT NOT NULL,
                    AgeAtSwitch INTEGER NOT NULL,
                    WeightAtSwitch TEXT NOT NULL,
                    RecommendedDosage TEXT NOT NULL,
                    DosageRationale TEXT NOT NULL,
                    ProtocolTimeline TEXT NOT NULL,
                    Contraindications TEXT NULL,
                    Warnings TEXT NULL,
                    ClinicalNotes TEXT NULL,
                    MonitoringPlan TEXT NULL,
                    FirstFollowUpDate TEXT NULL,
                    FollowUpCompleted INTEGER NOT NULL DEFAULT 0,
                    FollowUpNotes TEXT NULL,
                    SwitchCompleted INTEGER NOT NULL DEFAULT 0,
                    CompletionDate TEXT NULL,
                    Outcome TEXT NULL,
                    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                    FOREIGN KEY (PatientId) REFERENCES Patients(Id) ON DELETE CASCADE
                );
            ");

            // L'indice IX_AdverseEvents_Severity è già creato nella sezione SQL sopra

            // Crea indici solo se non esistono
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_PreTaoAssessments_AssessmentDate ON PreTaoAssessments (AssessmentDate);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_PreTaoAssessments_PatientId ON PreTaoAssessments (PatientId);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_TherapySwitches_FirstFollowUpDate_FollowUpCompleted ON TherapySwitches (FirstFollowUpDate, FollowUpCompleted);");
            migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS IX_TherapySwitches_PatientId_SwitchDate ON TherapySwitches (PatientId, SwitchDate DESC);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreTaoAssessments");

            migrationBuilder.DropTable(
                name: "TherapySwitches");

            migrationBuilder.DropIndex(
                name: "IX_AdverseEvents_Severity",
                table: "AdverseEvents");

            migrationBuilder.DropColumn(
                name: "ReactionType",
                table: "AdverseEvents");

            migrationBuilder.RenameColumn(
                name: "OnsetDate",
                table: "AdverseEvents",
                newName: "EventDate");

            migrationBuilder.RenameColumn(
                name: "MeasuresTaken",
                table: "AdverseEvents",
                newName: "Management");

            migrationBuilder.RenameColumn(
                name: "CertaintyLevel",
                table: "AdverseEvents",
                newName: "EventType");

            migrationBuilder.AlterColumn<string>(
                name: "Street",
                table: "DoctorData",
                type: "TEXT",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "PostalCode",
                table: "DoctorData",
                type: "TEXT",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "DoctorData",
                type: "TEXT",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "DoctorData",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "DoctorData",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "HemorrhagicCategory",
                table: "AdverseEvents",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "AdverseEvents",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThromboticCategory",
                table: "AdverseEvents",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeeklyDoseAtEvent",
                table: "AdverseEvents",
                type: "TEXT",
                precision: 5,
                scale: 2,
                nullable: true);
        }
    }
}
