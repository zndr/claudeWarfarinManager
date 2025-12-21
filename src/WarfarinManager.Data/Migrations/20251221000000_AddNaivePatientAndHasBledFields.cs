using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarfarinManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNaivePatientAndHasBledFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Aggiunge campo IsNaive per identificare pazienti in fase di induzione
            migrationBuilder.AddColumn<bool>(
                name: "IsNaive",
                table: "Patients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // Aggiunge campi HAS-BLED per calcolo rischio emorragico

            migrationBuilder.AddColumn<bool>(
                name: "HasRenalDisease",
                table: "Patients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasLiverDisease",
                table: "Patients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasStroke",
                table: "Patients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasBleedingHistory",
                table: "Patients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasLabileINR",
                table: "Patients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UsesDrugsIncreasingBleedingRisk",
                table: "Patients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UsesAlcohol",
                table: "Patients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNaive",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "HasRenalDisease",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "HasLiverDisease",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "HasStroke",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "HasBleedingHistory",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "HasLabileINR",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "UsesDrugsIncreasingBleedingRisk",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "UsesAlcohol",
                table: "Patients");
        }
    }
}
