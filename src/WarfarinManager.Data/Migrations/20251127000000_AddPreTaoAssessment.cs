using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarfarinManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPreTaoAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreTaoAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "TEXT", nullable: false),

                    // CHA2DS2VASc Components
                    CongestiveHeartFailure = table.Column<bool>(type: "INTEGER", nullable: false),
                    Hypertension = table.Column<bool>(type: "INTEGER", nullable: false),
                    Age75OrMore = table.Column<bool>(type: "INTEGER", nullable: false),
                    Diabetes = table.Column<bool>(type: "INTEGER", nullable: false),
                    PriorStrokeTiaTE = table.Column<bool>(type: "INTEGER", nullable: false),
                    VascularDisease = table.Column<bool>(type: "INTEGER", nullable: false),
                    Age65To74 = table.Column<bool>(type: "INTEGER", nullable: false),
                    Female = table.Column<bool>(type: "INTEGER", nullable: false),

                    // HAS-BLED Components
                    HasBledHypertension = table.Column<bool>(type: "INTEGER", nullable: false),
                    AbnormalRenalFunction = table.Column<bool>(type: "INTEGER", nullable: false),
                    AbnormalLiverFunction = table.Column<bool>(type: "INTEGER", nullable: false),
                    StrokeHistory = table.Column<bool>(type: "INTEGER", nullable: false),
                    BleedingHistory = table.Column<bool>(type: "INTEGER", nullable: false),
                    LabileINR = table.Column<bool>(type: "INTEGER", nullable: false),
                    Elderly = table.Column<bool>(type: "INTEGER", nullable: false),
                    DrugsPredisposing = table.Column<bool>(type: "INTEGER", nullable: false),
                    AlcoholAbuse = table.Column<bool>(type: "INTEGER", nullable: false),

                    // Absolute Contraindications
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

                    // Relative Contraindications
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

                    // Adverse Event Risk Factors
                    Polypharmacy = table.Column<bool>(type: "INTEGER", nullable: false),
                    SocialIsolation = table.Column<bool>(type: "INTEGER", nullable: false),
                    KnownDrugInteractions = table.Column<bool>(type: "INTEGER", nullable: false),
                    IrregularDietOrHighVitaminK = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExtremeBMI = table.Column<bool>(type: "INTEGER", nullable: false),
                    ChronicAnemia = table.Column<bool>(type: "INTEGER", nullable: false),
                    ActiveCancer = table.Column<bool>(type: "INTEGER", nullable: false),
                    ScheduledInvasiveProcedure = table.Column<bool>(type: "INTEGER", nullable: false),
                    KnownGeneticVariants = table.Column<bool>(type: "INTEGER", nullable: false),

                    // Notes and Assessment
                    ClinicalNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Recommendations = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    IsApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AssessingPhysician = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),

                    // BaseEntity fields
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

            migrationBuilder.CreateIndex(
                name: "IX_PreTaoAssessments_PatientId",
                table: "PreTaoAssessments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PreTaoAssessments_AssessmentDate",
                table: "PreTaoAssessments",
                column: "AssessmentDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreTaoAssessments");
        }
    }
}
