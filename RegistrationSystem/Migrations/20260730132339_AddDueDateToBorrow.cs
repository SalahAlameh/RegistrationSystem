using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDueDateToBorrow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Borrows",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql(
            """
    UPDATE [Borrows]
    SET [DueDate] = DATEADD(day, 14, [BorrowDate])
    WHERE [DueDate] IS NULL;
    """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DueDate",
                table: "Borrows",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Borrows");
        }
    }
}
