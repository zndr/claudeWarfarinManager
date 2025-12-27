using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarfarinManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicationMillepsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================================
            // NOTA: Tutte le colonne e tabelle di questa migrazione sono già gestite
            // da FixLegacyDatabaseSchemaAsync in App.xaml.cs che viene eseguito
            // PRIMA di MigrateAsync(). Questa migrazione usa solo operazioni idempotenti
            // (CREATE INDEX IF NOT EXISTS) per aggiungere gli indici.
            // Le colonne e tabelle sono aggiunte dal legacy fix, questa migrazione
            // serve solo per creare gli indici e essere registrata come applicata.
            // ============================================================================

            // Indice per Medications - AtcCode (usa Sql per essere idempotente)
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_Medications_Patient_AtcCode
                ON Medications(PatientId, AtcCode);
            ");

            // Indici per DoacMonitoring (la tabella è creata da FixLegacyDatabaseSchemaAsync)
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_DoacMonitoring_DataProssimoControllo
                ON DoacMonitoring(DataProssimoControllo)
                WHERE DataProssimoControllo IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_DoacMonitoring_PatientId_DataRilevazione
                ON DoacMonitoring(PatientId, DataRilevazione DESC);
            ");

            // Indici per TerapieContinuative (la tabella è creata da FixLegacyDatabaseSchemaAsync)
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_TerapieContinuative_Attiva_PatientId
                ON TerapieContinuative(Attiva, PatientId);
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_TerapieContinuative_Classe_PatientId_Attiva
                ON TerapieContinuative(Classe, PatientId, Attiva);
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_TerapieContinuative_PatientId
                ON TerapieContinuative(PatientId);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoacMonitoring");

            migrationBuilder.DropTable(
                name: "TerapieContinuative");

            migrationBuilder.DropIndex(
                name: "IX_Medications_Patient_AtcCode",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "HeightLastUpdated",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "WeightLastUpdated",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ActiveIngredient",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "AtcCode",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Medications");
        }
    }
}
