using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationSystem.Migrations
{
    /// <inheritdoc />
    public partial class ConvertExistingDatesToUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
    """
    UPDATE [Persons] SET [CreatedAt] =
        CONVERT(datetime2, ([CreatedAt] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [CreatedAt] > '0001-01-02';

    UPDATE [Persons] SET [UpdatedAt] =
        CONVERT(datetime2, ([UpdatedAt] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [UpdatedAt] > '0001-01-02';

    UPDATE [Persons] SET [DeletedAt] =
        CONVERT(datetime2, ([DeletedAt] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [DeletedAt] > '0001-01-02';

    UPDATE [Books] SET [CreatedAt] =
        CONVERT(datetime2, ([CreatedAt] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [CreatedAt] > '0001-01-02';

    UPDATE [Books] SET [UpdatedAt] =
        CONVERT(datetime2, ([UpdatedAt] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [UpdatedAt] > '0001-01-02';

    UPDATE [Books] SET [DeletedAt] =
        CONVERT(datetime2, ([DeletedAt] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [DeletedAt] > '0001-01-02';

    UPDATE [Authors] SET [CreatedAt] =
        CONVERT(datetime2, ([CreatedAt] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [CreatedAt] > '0001-01-02';

    UPDATE [Authors] SET [UpdatedAt] =
        CONVERT(datetime2, ([UpdatedAt] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [UpdatedAt] > '0001-01-02';

    UPDATE [Authors] SET [DeletedAt] =
        CONVERT(datetime2, ([DeletedAt] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [DeletedAt] > '0001-01-02';

    UPDATE [Categories] SET [CreatedAt] =
        CONVERT(datetime2, ([CreatedAt] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [CreatedAt] > '0001-01-02';

    UPDATE [Categories] SET [UpdatedAt] =
        CONVERT(datetime2, ([UpdatedAt] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [UpdatedAt] > '0001-01-02';

    UPDATE [Categories] SET [DeletedAt] =
        CONVERT(datetime2, ([DeletedAt] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [DeletedAt] > '0001-01-02';

    UPDATE [Borrows] SET [CreatedAt] =
        CONVERT(datetime2, ([CreatedAt] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [CreatedAt] > '0001-01-02';

    UPDATE [Borrows] SET [UpdatedAt] =
        CONVERT(datetime2, ([UpdatedAt] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [UpdatedAt] > '0001-01-02';

    UPDATE [Borrows] SET [BorrowDate] =
        CONVERT(datetime2, ([BorrowDate] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [BorrowDate] > '0001-01-02';

    UPDATE [Borrows] SET [ReturnDate] =
        CONVERT(datetime2, ([ReturnDate] AT TIME ZONE 'Middle East Standard Time') AT TIME ZONE 'UTC')
    WHERE [ReturnDate] > '0001-01-02';
    """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // UTC conversion is intentionally not reversed because new records
            // created after this migration are already stored in UTC.

        }
    }
}
