using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarfarinManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMillewinCodeToDoctorData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NO-OP: La colonna MillewinCode viene già creata in modo idempotente
            // da App.xaml.cs -> FixLegacyDatabaseSchemaAsync() -> EnsureColumnExistsAsync()
            // prima che MigrateAsync() venga chiamato.
            // Questa migration serve solo per aggiornare lo snapshot del modello.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // NO-OP
        }
    }
}
