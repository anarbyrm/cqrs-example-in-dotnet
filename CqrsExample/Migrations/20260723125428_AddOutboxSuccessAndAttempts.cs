using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CqrsExample.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxSuccessAndAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProcessAttempts",
                table: "Outbox",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Success",
                table: "Outbox",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessAttempts",
                table: "Outbox");

            migrationBuilder.DropColumn(
                name: "Success",
                table: "Outbox");
        }
    }
}
