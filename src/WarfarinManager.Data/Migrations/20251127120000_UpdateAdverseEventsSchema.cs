using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarfarinManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdverseEventsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old table if it exists
            migrationBuilder.DropTable(
                name: "AdverseEvents",
                schema: null);

            // Create new AdverseEvents table with updated schema
            migrationBuilder.CreateTable(
                name: "AdverseEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                    OnsetDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReactionType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CertaintyLevel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MeasuresTaken = table.Column<string>(type: "TEXT", nullable: true),
                    INRAtEvent = table.Column<decimal>(type: "TEXT", precision: 4, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    LinkedINRControlId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdverseEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdverseEvents_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdverseEvents_INRControls_LinkedINRControlId",
                        column: x => x.LinkedINRControlId,
                        principalTable: "INRControls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdverseEvents_Patient_Date",
                table: "AdverseEvents",
                columns: new[] { "PatientId", "OnsetDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_AdverseEvents_Severity",
                table: "AdverseEvents",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_AdverseEvents_LinkedINRControlId",
                table: "AdverseEvents",
                column: "LinkedINRControlId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdverseEvents");
        }
    }
}
