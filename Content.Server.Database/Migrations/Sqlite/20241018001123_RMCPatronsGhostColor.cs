// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class RMCPatronsGhostColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ghost_color",
                table: "rmc_patrons",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ghost_color",
                table: "rmc_patron_tiers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ghost_color",
                table: "rmc_patrons");

            migrationBuilder.DropColumn(
                name: "ghost_color",
                table: "rmc_patron_tiers");
        }
    }
}
