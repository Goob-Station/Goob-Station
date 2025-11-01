﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class Achievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "achievements",
                columns: table => new
                {
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    achievement_id = table.Column<string>(type: "text", nullable: false),
                    player_id1 = table.Column<int>(type: "integer", nullable: false),
                    progress = table.Column<float>(type: "real", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_achievements", x => new { x.player_id, x.achievement_id });
                    table.ForeignKey(
                        name: "FK_achievements_player_player_id1",
                        column: x => x.player_id1,
                        principalTable: "player",
                        principalColumn: "player_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_achievements_achievement_id",
                table: "achievements",
                column: "achievement_id");

            migrationBuilder.CreateIndex(
                name: "IX_achievements_player_id",
                table: "achievements",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_achievements_player_id1",
                table: "achievements",
                column: "player_id1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "achievements");
        }
    }
}
