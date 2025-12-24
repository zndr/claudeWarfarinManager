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
            // Per SQLite, dobbiamo ricreare la tabella AdverseEvents con il nuovo schema
            // perché SQLite non supporta bene ALTER TABLE per rinominare colonne o modifiche complesse

            // 1. Crea tabella temporanea con il nuovo schema
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS AdverseEvents_new (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    PatientId INTEGER NOT NULL,
                    OnsetDate TEXT NOT NULL,
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

            // 2. Copia i dati esistenti (mapping delle colonne vecchie alle nuove)
            migrationBuilder.Sql(@"
                INSERT INTO AdverseEvents_new (Id, PatientId, OnsetDate, ReactionType, Severity, CertaintyLevel, MeasuresTaken, INRAtEvent, Notes, LinkedINRControlId, CreatedAt, UpdatedAt)
                SELECT
                    Id,
                    PatientId,
                    COALESCE(EventDate, datetime('now')),
                    COALESCE(EventType, 'Other'),
                    Severity,
                    'Possible',
                    Management,
                    INRAtEvent,
                    Notes,
                    LinkedINRControlId,
                    CreatedAt,
                    UpdatedAt
                FROM AdverseEvents;
            ");

            // 3. Elimina la vecchia tabella
            migrationBuilder.Sql("DROP TABLE AdverseEvents;");

            // 4. Rinomina la nuova tabella
            migrationBuilder.Sql("ALTER TABLE AdverseEvents_new RENAME TO AdverseEvents;");

            // 5. Ricrea gli indici
            migrationBuilder.Sql("CREATE INDEX IX_AdverseEvents_Patient_Date ON AdverseEvents (PatientId, OnsetDate DESC);");
            migrationBuilder.Sql("CREATE INDEX IX_AdverseEvents_Severity ON AdverseEvents (Severity);");
            migrationBuilder.Sql("CREATE INDEX IX_AdverseEvents_LinkedINRControlId ON AdverseEvents (LinkedINRControlId);");

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

            migrationBuilder.CreateTable(
                name: "PreTaoAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CongestiveHeartFailure = table.Column<bool>(type: "INTEGER", nullable: false),
                    Hypertension = table.Column<bool>(type: "INTEGER", nullable: false),
                    Age75OrMore = table.Column<bool>(type: "INTEGER", nullable: false),
                    Diabetes = table.Column<bool>(type: "INTEGER", nullable: false),
                    PriorStrokeTiaTE = table.Column<bool>(type: "INTEGER", nullable: false),
                    VascularDisease = table.Column<bool>(type: "INTEGER", nullable: false),
                    Age65To74 = table.Column<bool>(type: "INTEGER", nullable: false),
                    Female = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasBledHypertension = table.Column<bool>(type: "INTEGER", nullable: false),
                    AbnormalRenalFunction = table.Column<bool>(type: "INTEGER", nullable: false),
                    AbnormalLiverFunction = table.Column<bool>(type: "INTEGER", nullable: false),
                    StrokeHistory = table.Column<bool>(type: "INTEGER", nullable: false),
                    BleedingHistory = table.Column<bool>(type: "INTEGER", nullable: false),
                    LabileINR = table.Column<bool>(type: "INTEGER", nullable: false),
                    Elderly = table.Column<bool>(type: "INTEGER", nullable: false),
                    DrugsPredisposing = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlcoholAbuse = table.Column<bool>(type: "INTEGER", nullable: false),
                    ActiveMajorBleeding = table.Column<bool>(type: "INTEGER", nullable: false),
                    Pregnancy = table.Column<bool>(type: "INTEGER", nullable: false),
                    SevereBloodDyscrasia = table.Column<bool>(type: "INTEGER", nullable: false),
                    RecentNeurosurgery = table.Column<bool>(type: "INTEGER", nullable: false),
                    IntracranialBleedingOrMalformation = table.Column<bool>(type: "INTEGER", nullable: false),
                    ActivePepticUlcerOrVarices = table.Column<bool>(type: "INTEGER", nullable: false),
                    AcuteBacterialEndocarditis = table.Column<bool>(type: "INTEGER", nullable: false),
                    SevereUncontrolledHypertension = table.Column<bool>(type: "INTEGER", nullable: false),
                    WarfarinAllergy = table.Column<bool>(type: "INTEGER", nullable: false),
                    LackOfCompliance = table.Column<bool>(type: "INTEGER", nullable: false),
                    RecentGIBleeding = table.Column<bool>(type: "INTEGER", nullable: false),
                    HistoryOfMajorBleeding = table.Column<bool>(type: "INTEGER", nullable: false),
                    ModerateRenalFailure = table.Column<bool>(type: "INTEGER", nullable: false),
                    ModerateHepaticFailure = table.Column<bool>(type: "INTEGER", nullable: false),
                    ModerateThrombocytopenia = table.Column<bool>(type: "INTEGER", nullable: false),
                    FrequentFalls = table.Column<bool>(type: "INTEGER", nullable: false),
                    CognitiveImpairment = table.Column<bool>(type: "INTEGER", nullable: false),
                    RecentMajorSurgery = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrganicLesionsAtRisk = table.Column<bool>(type: "INTEGER", nullable: false),
                    AcutePericarditis = table.Column<bool>(type: "INTEGER", nullable: false),
                    Polypharmacy = table.Column<bool>(type: "INTEGER", nullable: false),
                    SocialIsolation = table.Column<bool>(type: "INTEGER", nullable: false),
                    KnownDrugInteractions = table.Column<bool>(type: "INTEGER", nullable: false),
                    IrregularDietOrHighVitaminK = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExtremeBMI = table.Column<bool>(type: "INTEGER", nullable: false),
                    ChronicAnemia = table.Column<bool>(type: "INTEGER", nullable: false),
                    ActiveCancer = table.Column<bool>(type: "INTEGER", nullable: false),
                    ScheduledInvasiveProcedure = table.Column<bool>(type: "INTEGER", nullable: false),
                    KnownGeneticVariants = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClinicalNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Recommendations = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AssessingPhysician = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreTaoAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreTaoAssessments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TherapySwitches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                    SwitchDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Direction = table.Column<string>(type: "TEXT", nullable: false),
                    DoacType = table.Column<string>(type: "TEXT", nullable: false),
                    WarfarinType = table.Column<string>(type: "TEXT", nullable: false),
                    InrAtSwitch = table.Column<decimal>(type: "TEXT", nullable: true),
                    CreatinineClearance = table.Column<decimal>(type: "TEXT", nullable: false),
                    AgeAtSwitch = table.Column<int>(type: "INTEGER", nullable: false),
                    WeightAtSwitch = table.Column<decimal>(type: "TEXT", nullable: false),
                    RecommendedDosage = table.Column<string>(type: "TEXT", nullable: false),
                    DosageRationale = table.Column<string>(type: "TEXT", nullable: false),
                    ProtocolTimeline = table.Column<string>(type: "TEXT", nullable: false),
                    Contraindications = table.Column<string>(type: "TEXT", nullable: true),
                    Warnings = table.Column<string>(type: "TEXT", nullable: true),
                    ClinicalNotes = table.Column<string>(type: "TEXT", nullable: true),
                    MonitoringPlan = table.Column<string>(type: "TEXT", nullable: true),
                    FirstFollowUpDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FollowUpCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    FollowUpNotes = table.Column<string>(type: "TEXT", nullable: true),
                    SwitchCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Outcome = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapySwitches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TherapySwitches_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // L'indice IX_AdverseEvents_Severity è già creato nella sezione SQL sopra

            migrationBuilder.CreateIndex(
                name: "IX_PreTaoAssessments_AssessmentDate",
                table: "PreTaoAssessments",
                column: "AssessmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_PreTaoAssessments_PatientId",
                table: "PreTaoAssessments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TherapySwitches_FirstFollowUpDate_FollowUpCompleted",
                table: "TherapySwitches",
                columns: new[] { "FirstFollowUpDate", "FollowUpCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_TherapySwitches_PatientId_SwitchDate",
                table: "TherapySwitches",
                columns: new[] { "PatientId", "SwitchDate" },
                descending: new[] { false, true });
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
