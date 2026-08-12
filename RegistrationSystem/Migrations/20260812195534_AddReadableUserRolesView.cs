using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddReadableUserRolesView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
            """
            CREATE VIEW [dbo].[UserRolesWithNames]
            AS
            SELECT
                [userRole].[UserId],
                [user].[UserName],
                [user].[Email],
                [userRole].[RoleId],
                [role].[Name] AS [RoleName]
            FROM [dbo].[AspNetUserRoles] AS [userRole]
            INNER JOIN [dbo].[AspNetUsers] AS [user]
                ON [userRole].[UserId] = [user].[Id]
            INNER JOIN [dbo].[AspNetRoles] AS [role]
                ON [userRole].[RoleId] = [role].[Id];
            """);


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
            "DROP VIEW [dbo].[UserRolesWithNames];");

        }
    }
}
