using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationSystem.Migrations
{
    /// <inheritdoc />
    public partial class ConfirmExistingUsersForEmailRecovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
        """
        UPDATE [AspNetUsers]
        SET [EmailConfirmed] = 1
        WHERE [EmailConfirmed] = 0;
        """);


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Existing accounts must remain confirmed after email recovery
            // is enabled, so this data change is intentionally not reversed.

        }
    }
}
