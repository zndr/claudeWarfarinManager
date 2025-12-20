using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarfarinManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTherapySwitchTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_TherapySwitches_PatientId_SwitchDate",
                table: "TherapySwitches",
                columns: new[] { "PatientId", "SwitchDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_TherapySwitches_FirstFollowUpDate_FollowUpCompleted",
                table: "TherapySwitches",
                columns: new[] { "FirstFollowUpDate", "FollowUpCompleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TherapySwitches");
        }
    }
}
