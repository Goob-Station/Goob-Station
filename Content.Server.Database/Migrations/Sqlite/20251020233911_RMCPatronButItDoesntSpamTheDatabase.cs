using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class RMCPatronButItDoesntSpamTheDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_rmc_patron_tiers_lobby_message",
                table: "rmc_patron_tiers",
                column: "lobby_message");

            migrationBuilder.CreateIndex(
                name: "IX_rmc_patron_tiers_round_end_shoutout",
                table: "rmc_patron_tiers",
                column: "round_end_shoutout");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_rmc_patron_tiers_lobby_message",
                table: "rmc_patron_tiers");

            migrationBuilder.DropIndex(
                name: "IX_rmc_patron_tiers_round_end_shoutout",
                table: "rmc_patron_tiers");
        }
    }
}
