using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddReturndateToBorrow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "Borrows",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "Borrows");
        }
    }
}
