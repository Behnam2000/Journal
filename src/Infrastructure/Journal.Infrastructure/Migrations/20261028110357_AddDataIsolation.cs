using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Journal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDataIsolation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "trades",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "user_id",
                table: "daily_journals",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_id",
                table: "trades");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "daily_journals");
        }
    }
}
