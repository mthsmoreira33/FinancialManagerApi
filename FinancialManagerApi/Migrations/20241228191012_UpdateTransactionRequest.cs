using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialManagerApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransactionRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBlacklisted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBlacklisted",
                table: "Users");
        }
    }
}
