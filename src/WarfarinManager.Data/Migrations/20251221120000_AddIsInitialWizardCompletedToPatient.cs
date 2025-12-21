using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarfarinManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsInitialWizardCompletedToPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Aggiunge campo IsInitialWizardCompleted per tracciare il completamento del wizard obbligatorio
            migrationBuilder.AddColumn<bool>(
                name: "IsInitialWizardCompleted",
                table: "Patients",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInitialWizardCompleted",
                table: "Patients");
        }
    }
}
