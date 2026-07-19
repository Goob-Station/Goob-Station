using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class GhostCosmetics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ghost_hat",
                table: "rmc_patrons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ghost_mask",
                table: "rmc_patrons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ghost_particles",
                table: "rmc_patrons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ghost_cosmetics",
                table: "rmc_patron_tiers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ghost_particles",
                table: "rmc_patron_tiers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ghost_hat",
                table: "rmc_patrons");

            migrationBuilder.DropColumn(
                name: "ghost_mask",
                table: "rmc_patrons");

            migrationBuilder.DropColumn(
                name: "ghost_particles",
                table: "rmc_patrons");

            migrationBuilder.DropColumn(
                name: "ghost_cosmetics",
                table: "rmc_patron_tiers");

            migrationBuilder.DropColumn(
                name: "ghost_particles",
                table: "rmc_patron_tiers");
        }
    }
}
