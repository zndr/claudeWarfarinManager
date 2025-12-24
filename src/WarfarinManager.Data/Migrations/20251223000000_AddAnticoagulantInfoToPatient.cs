using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarfarinManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAnticoagulantInfoToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Aggiunge campo AnticoagulantType per memorizzare il tipo di anticoagulante
            migrationBuilder.AddColumn<string>(
                name: "AnticoagulantType",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            // Aggiunge campo TherapyStartDate per memorizzare la data di inizio terapia
            migrationBuilder.AddColumn<DateTime>(
                name: "TherapyStartDate",
                table: "Patients",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnticoagulantType",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "TherapyStartDate",
                table: "Patients");
        }
    }
}
