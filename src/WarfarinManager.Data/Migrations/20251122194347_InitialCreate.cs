using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WarfarinManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndicationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TargetINRMin = table.Column<decimal>(type: "TEXT", precision: 3, scale: 1, nullable: false),
                    TargetINRMax = table.Column<decimal>(type: "TEXT", precision: 3, scale: 1, nullable: false),
                    TypicalDuration = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndicationTypes", x => x.Id);
                    table.UniqueConstraint("AK_IndicationTypes_Code", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "InteractionDrugs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DrugName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    InteractionEffect = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    OddsRatio = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    Mechanism = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FCSAManagement = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ACCPManagement = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    RecommendedINRCheckDays = table.Column<int>(type: "INTEGER", nullable: true),
                    InteractionLevel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionDrugs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FiscalCode = table.Column<string>(type: "TEXT", fixedLength: true, maxLength: 16, nullable: false),
                    Gender = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    IsSlowMetabolizer = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BridgeTherapyPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                    SurgeryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SurgeryType = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    ThromboembolicRisk = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    BridgeRecommended = table.Column<bool>(type: "INTEGER", nullable: false),
                    WarfarinStopDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EBPMStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EBPMLastDoseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    WarfarinResumeDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EBPMResumeDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProtocolText = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BridgeTherapyPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BridgeTherapyPlans_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Indications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                    IndicationTypeCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TargetINRMin = table.Column<decimal>(type: "TEXT", precision: 3, scale: 1, nullable: false),
                    TargetINRMax = table.Column<decimal>(type: "TEXT", precision: 3, scale: 1, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ChangeReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Indications_IndicationTypes_IndicationTypeCode",
                        column: x => x.IndicationTypeCode,
                        principalTable: "IndicationTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Indications_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "INRControls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                    ControlDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    INRValue = table.Column<decimal>(type: "TEXT", precision: 4, scale: 2, nullable: false),
                    CurrentWeeklyDose = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    PhaseOfTherapy = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsCompliant = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_INRControls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_INRControls_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                    MedicationName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Dosage = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Frequency = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    InteractionLevel = table.Column<string>(type: "TEXT", nullable: false),
                    InteractionDetails = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medications_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdverseEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                    EventDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    HemorrhagicCategory = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    ThromboticCategory = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    INRAtEvent = table.Column<decimal>(type: "TEXT", precision: 4, scale: 2, nullable: true),
                    WeeklyDoseAtEvent = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    Management = table.Column<string>(type: "TEXT", nullable: true),
                    Outcome = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    LinkedINRControlId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdverseEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdverseEvents_INRControls_LinkedINRControlId",
                        column: x => x.LinkedINRControlId,
                        principalTable: "INRControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AdverseEvents_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyDoses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    INRControlId = table.Column<int>(type: "INTEGER", nullable: false),
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: false),
                    DoseMg = table.Column<decimal>(type: "TEXT", precision: 4, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyDoses", x => x.Id);
                    table.CheckConstraint("CK_DailyDoses_DayOfWeek", "[DayOfWeek] BETWEEN 1 AND 7");
                    table.ForeignKey(
                        name: "FK_DailyDoses_INRControls_INRControlId",
                        column: x => x.INRControlId,
                        principalTable: "INRControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DosageSuggestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    INRControlId = table.Column<int>(type: "INTEGER", nullable: false),
                    GuidelineUsed = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SuggestedWeeklyDose = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    LoadingDoseAction = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PercentageAdjustment = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    NextControlDays = table.Column<int>(type: "INTEGER", nullable: false),
                    RequiresEBPM = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    RequiresVitaminK = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    WeeklySchedule = table.Column<string>(type: "TEXT", nullable: false),
                    ClinicalNotes = table.Column<string>(type: "TEXT", nullable: true),
                    ExportedText = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DosageSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DosageSuggestions_INRControls_INRControlId",
                        column: x => x.INRControlId,
                        principalTable: "INRControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "IndicationTypes",
                columns: new[] { "Id", "Category", "Code", "Description", "TargetINRMax", "TargetINRMin", "TypicalDuration" },
                values: new object[,]
                {
                    { 1, "Tromboembolismo Venoso", "TVP_TREATMENT", "TVP - Trombosi Venosa Profonda (trattamento)", 3.0m, 2.0m, "3-6 mesi (primo episodio), indefinito se ricorrente" },
                    { 2, "Tromboembolismo Venoso", "TVP_PROPHYLAXIS", "TVP - Profilassi tromboembolica", 3.0m, 2.0m, "Secondo fattori di rischio" },
                    { 3, "Tromboembolismo Venoso", "PE_TREATMENT", "Embolia Polmonare (trattamento)", 3.0m, 2.0m, "3-6 mesi (primo episodio), indefinito se ricorrente" },
                    { 4, "Tromboembolismo Venoso", "TVP_PE_RECURRENT", "TVP/EP ricorrente", 3.0m, 2.0m, "Indefinito (lifelong)" },
                    { 5, "Fibrillazione Atriale", "FA_STROKE_PREVENTION", "Fibrillazione Atriale - Prevenzione stroke (CHA₂DS₂-VASc ≥2)", 3.0m, 2.0m, "Indefinito" },
                    { 6, "Fibrillazione Atriale", "FA_MITRAL_STENOSIS", "FA con stenosi mitralica", 3.0m, 2.0m, "Indefinito" },
                    { 7, "Fibrillazione Atriale", "FA_VALVE_PROSTHESIS", "FA con protesi valvolare", 3.5m, 2.5m, "Indefinito" },
                    { 8, "Protesi Valvolari", "MECHANICAL_MITRAL", "Protesi meccanica mitralica", 3.5m, 2.5m, "Indefinito (lifelong)" },
                    { 9, "Protesi Valvolari", "MECHANICAL_AORTIC", "Protesi meccanica aortica", 3.5m, 2.5m, "Indefinito (lifelong)" },
                    { 10, "Protesi Valvolari", "MECHANICAL_TRICUSPID", "Protesi meccanica tricuspidale", 3.5m, 2.5m, "Indefinito (lifelong)" },
                    { 11, "Protesi Valvolari", "BIOPROSTHESIS_EARLY", "Bioprotesi valvolare (primi 3-6 mesi)", 3.0m, 2.0m, "3-6 mesi post-impianto" },
                    { 12, "Infarto Miocardico", "MI_LV_DYSFUNCTION", "Post-IM con disfunzione ventricolo sinistro (FE <35%)", 3.0m, 2.0m, "3-6 mesi, poi rivalutare" },
                    { 13, "Infarto Miocardico", "MI_ANTERIOR_EXTENSIVE", "Infarto miocardico anteriore esteso", 3.0m, 2.0m, "3-6 mesi" },
                    { 14, "Infarto Miocardico", "LV_ANEURYSM", "Aneurisma ventricolare sinistro", 3.0m, 2.0m, "Indefinito" },
                    { 15, "Infarto Miocardico", "LV_THROMBUS", "Trombo intraventricolare", 3.0m, 2.0m, "3-6 mesi (fino a risoluzione trombo)" },
                    { 16, "Cardiomiopatie", "DILATED_CARDIOMYOPATHY", "Cardiomiopatia dilatativa con FE ridotta (<35%)", 3.0m, 2.0m, "Indefinito se alto rischio TE" },
                    { 17, "Sindrome Anticorpi Antifosfolipidi", "APS_ARTERIAL", "Sindrome anticorpi antifosfolipidi (eventi arteriosi)", 3.0m, 2.0m, "Indefinito (lifelong)" },
                    { 18, "Sindrome Anticorpi Antifosfolipidi", "APS_VENOUS", "Sindrome anticorpi antifosfolipidi (eventi venosi)", 3.0m, 2.0m, "Indefinito (lifelong)" },
                    { 19, "Sindrome Anticorpi Antifosfolipidi", "APS_RECURRENT", "APS con eventi ricorrenti nonostante INR 2-3", 3.5m, 2.5m, "Indefinito (target più alto)" },
                    { 20, "Stroke/TIA", "ISCHEMIC_STROKE", "Ictus ischemico cardioembolico", 3.0m, 2.0m, "Indefinito se FA o altra cardiopatia emboligena" },
                    { 21, "Stroke/TIA", "TIA_CARDIOEMBOLIC", "TIA cardioembolico", 3.0m, 2.0m, "Indefinito se FA o altra cardiopatia emboligena" },
                    { 22, "Altre Indicazioni", "PULMONARY_HYPERTENSION", "Ipertensione polmonare primaria", 3.0m, 2.0m, "Indefinito" },
                    { 23, "Altre Indicazioni", "PERIPHERAL_ARTERIAL_DISEASE", "Arteriopatia periferica severa", 3.0m, 2.0m, "Secondo valutazione vascolare" }
                });

            migrationBuilder.InsertData(
                table: "InteractionDrugs",
                columns: new[] { "Id", "ACCPManagement", "Category", "DrugName", "FCSAManagement", "InteractionEffect", "InteractionLevel", "Mechanism", "OddsRatio", "RecommendedINRCheckDays" },
                values: new object[,]
                {
                    { 1, "Monitoraggio stretto INR. Considerare antibiotico alternativo.", "Antibiotic", "Cotrimoxazolo (Trimetoprim-Sulfametoxazolo)", "Ridurre dose warfarin 25-40% se indispensabile. Controllo INR dopo 3-5 giorni.", "Increases", "High", "Inibizione CYP2C9 + inibizione sintesi Vitamina K", 2.70m, 3 },
                    { 2, "Riduzione empirica dose. Monitoraggio stretto.", "Antifungal", "Fluconazolo", "Ridurre dose warfarin 25-40%. Controllo INR ravvicinato ogni 3-5 giorni.", "Increases", "High", "Inibizione CYP2C9", 4.57m, 3 },
                    { 3, "Riduzione significativa dose. Controlli frequenti.", "Antifungal", "Voriconazolo", "Ridurre dose warfarin 25-40%. Monitoraggio giornaliero INR inizialmente.", "Increases", "High", "Inibizione CYP2C9", 4.57m, 3 },
                    { 4, "Riduzione dose empirica 30-50%. Monitoraggio stretto.", "Antibiotic", "Metronidazolo", "Ridurre dose warfarin 1/3-1/2 se necessario. Controllo INR dopo 3 giorni.", "Increases", "High", "Inibizione CYP2C9", null, 3 },
                    { 5, "Controllo INR entro 3-5 giorni dall'inizio.", "Antibiotic", "Eritromicina", "Monitoraggio INR. Ridurre dose 10-25% se necessario.", "Increases", "High", "Inibizione CYP3A4", 1.86m, 5 },
                    { 6, "Controllo INR entro 3-5 giorni.", "Antibiotic", "Claritromicina", "Monitoraggio INR. Ridurre dose 10-25% se necessario.", "Increases", "High", "Inibizione CYP3A4", 1.86m, 5 },
                    { 7, "Controllo INR dopo 5-7 giorni.", "Antibiotic", "Ciprofloxacina", "Monitoraggio INR. Ridurre dose 10-15% se necessario.", "Increases", "Moderate", "Meccanismo variabile", 1.69m, 5 },
                    { 8, "Controllo INR dopo 5-7 giorni.", "Antibiotic", "Levofloxacina", "Monitoraggio INR. Ridurre dose 10-15% se necessario.", "Increases", "Moderate", "Meccanismo variabile", 1.69m, 5 },
                    { 9, "Riduzione empirica 25-30%. Controlli settimanali per 6-8 settimane.", "Antiarrhythmic", "Amiodarone", "Ridurre dose warfarin 20-30% all'inizio. Aumentare 60% alla sospensione.", "Increases", "High", "Inibizione CYP2C9 + lunga emivita (40-60 giorni)", null, 7 },
                    { 10, "Controllo INR dopo 7 giorni se terapia prolungata.", "Antibiotic", "Azitromicina", "Monitoraggio routine. Generalmente non richiede aggiustamento.", "Increases", "Low", "Interazione minore", null, 7 },
                    { 11, "Aumentare dose 50-100%. Controllo INR ogni 3-5 giorni.", "Antibiotic", "Rifampicina", "Aumentare dose warfarin fino a 100%. Riduzione rapida in 5-8 giorni dopo stop.", "Decreases", "High", "Induzione CYP2C9", null, 3 },
                    { 12, "Aumento significativo dose. Controlli frequenti.", "Anticonvulsant", "Carbamazepina", "Aumentare dose warfarin 50-100%. Monitoraggio stretto.", "Decreases", "High", "Induzione CYP2C9", null, 5 },
                    { 13, "Aumento significativo dose. Controlli frequenti.", "Anticonvulsant", "Fenobarbital", "Aumentare dose warfarin 50-100%. Monitoraggio stretto.", "Decreases", "High", "Induzione CYP2C9", null, 5 },
                    { 14, "Controlli INR frequenti. Effetto imprevedibile.", "Anticonvulsant", "Fenitoina", "Effetto bifasico: inizialmente aumenta INR, poi lo riduce. Monitoraggio stretto.", "Variable", "High", "Induzione CYP2C9 + competizione legame proteine", null, 3 },
                    { 15, "Sconsigliato uso cronico. Aumenta rischio emorragico.", "NSAID", "FANS (Ibuprofene, Diclofenac, Naprossene)", "Evitare se possibile. Preferire paracetamolo.", "Increases", "Moderate", "Effetto antiaggregante + gastropatia", null, 7 },
                    { 16, "Aumenta rischio emorragico. Usare solo se indicato.", "Antiplatelet", "Aspirina (>100mg/die)", "Valutare rischio/beneficio. Dosi basse. Gastroprotettore.", "Increases", "Moderate", "Effetto antiaggregante sinergico", null, 7 },
                    { 17, "Nessun aggiustamento routinario necessario.", "PPI", "Omeprazolo", "Interazione clinicamente poco rilevante. Monitoraggio routine.", "Increases", "Low", "Inibizione minore CYP2C19", null, 14 },
                    { 18, "Interazione clinicamente non significativa.", "Statin", "Simvastatina", "Monitoraggio routine. Raramente richiede aggiustamento.", "Increases", "Low", "Interazione minore", null, 14 },
                    { 19, "Controllo INR dopo 2-4 settimane da modifiche dosaggio.", "Thyroid", "Levotiroxina", "Monitoraggio INR se cambio dosaggio tiroxina.", "Increases", "Low", "Aumento catabolismo fattori coagulazione", null, 14 },
                    { 20, "Limitare consumo. Effetto imprevedibile.", "Other", "Alcool (uso cronico/abuso)", "Educare paziente: max 1-2 unità/die.", "Variable", "Moderate", "Uso acuto aumenta INR, uso cronico induce enzimi", null, 7 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdverseEvents_LinkedINRControlId",
                table: "AdverseEvents",
                column: "LinkedINRControlId");

            migrationBuilder.CreateIndex(
                name: "IX_AdverseEvents_Patient_Date",
                table: "AdverseEvents",
                columns: new[] { "PatientId", "EventDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_BridgeTherapyPlans_PatientId",
                table: "BridgeTherapyPlans",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyDoses_INRControlId",
                table: "DailyDoses",
                column: "INRControlId");

            migrationBuilder.CreateIndex(
                name: "IX_DosageSuggestions_INRControlId",
                table: "DosageSuggestions",
                column: "INRControlId");

            migrationBuilder.CreateIndex(
                name: "IX_Indications_IndicationTypeCode",
                table: "Indications",
                column: "IndicationTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_Indications_Patient_Active",
                table: "Indications",
                columns: new[] { "PatientId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_IndicationTypes_Code",
                table: "IndicationTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_INRControls_Patient_Date",
                table: "INRControls",
                columns: new[] { "PatientId", "ControlDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_InteractionDrugs_DrugName",
                table: "InteractionDrugs",
                column: "DrugName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medications_Patient_Active",
                table: "Medications",
                columns: new[] { "PatientId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_FiscalCode",
                table: "Patients",
                column: "FiscalCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Name",
                table: "Patients",
                columns: new[] { "LastName", "FirstName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdverseEvents");

            migrationBuilder.DropTable(
                name: "BridgeTherapyPlans");

            migrationBuilder.DropTable(
                name: "DailyDoses");

            migrationBuilder.DropTable(
                name: "DosageSuggestions");

            migrationBuilder.DropTable(
                name: "Indications");

            migrationBuilder.DropTable(
                name: "InteractionDrugs");

            migrationBuilder.DropTable(
                name: "Medications");

            migrationBuilder.DropTable(
                name: "INRControls");

            migrationBuilder.DropTable(
                name: "IndicationTypes");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
