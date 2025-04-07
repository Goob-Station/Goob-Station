// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    public partial class AdminLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "ak_player_user_id",
                table: "player",
                column: "user_id");

            migrationBuilder.CreateTable(
                name: "round",
                columns: table => new
                {
                    round_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_round", x => x.round_id);
                });

            migrationBuilder.CreateTable(
                name: "admin_log",
                columns: table => new
                {
                    admin_log_id = table.Column<int>(type: "INTEGER", nullable: false),
                    round_id = table.Column<int>(type: "INTEGER", nullable: false),
                    type = table.Column<int>(type: "INTEGER", nullable: false),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    message = table.Column<string>(type: "TEXT", nullable: false),
                    json = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_log", x => new { x.admin_log_id, x.round_id });
                    table.ForeignKey(
                        name: "FK_admin_log_round_round_id",
                        column: x => x.round_id,
                        principalTable: "round",
                        principalColumn: "round_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_round",
                columns: table => new
                {
                    players_id = table.Column<int>(type: "INTEGER", nullable: false),
                    rounds_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_round", x => new { x.players_id, x.rounds_id });
                    table.ForeignKey(
                        name: "FK_player_round_player_players_id",
                        column: x => x.players_id,
                        principalTable: "player",
                        principalColumn: "player_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_player_round_round_rounds_id",
                        column: x => x.rounds_id,
                        principalTable: "round",
                        principalColumn: "round_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "admin_log_entity",
                columns: table => new
                {
                    uid = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    admin_log_id = table.Column<int>(type: "INTEGER", nullable: true),
                    admin_log_round_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_log_entity", x => x.uid);
                    table.ForeignKey(
                        name: "FK_admin_log_entity_admin_log_admin_log_id_admin_log_round_id",
                        columns: x => new { x.admin_log_id, x.admin_log_round_id },
                        principalTable: "admin_log",
                        principalColumns: new[] { "admin_log_id", "round_id" });
                });

            migrationBuilder.CreateTable(
                name: "admin_log_player",
                columns: table => new
                {
                    player_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    log_id = table.Column<int>(type: "INTEGER", nullable: false),
                    round_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_log_player", x => new { x.player_user_id, x.log_id, x.round_id });
                    table.ForeignKey(
                        name: "FK_admin_log_player_admin_log_log_id_round_id",
                        columns: x => new { x.log_id, x.round_id },
                        principalTable: "admin_log",
                        principalColumns: new[] { "admin_log_id", "round_id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_admin_log_player_player_player_user_id",
                        column: x => x.player_user_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_log_round_id",
                table: "admin_log",
                column: "round_id");

            migrationBuilder.CreateIndex(
                name: "IX_admin_log_type",
                table: "admin_log",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_admin_log_entity_admin_log_id_admin_log_round_id",
                table: "admin_log_entity",
                columns: new[] { "admin_log_id", "admin_log_round_id" });

            migrationBuilder.CreateIndex(
                name: "IX_admin_log_player_log_id_round_id",
                table: "admin_log_player",
                columns: new[] { "log_id", "round_id" });

            migrationBuilder.CreateIndex(
                name: "IX_player_round_rounds_id",
                table: "player_round",
                column: "rounds_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_log_entity");

            migrationBuilder.DropTable(
                name: "admin_log_player");

            migrationBuilder.DropTable(
                name: "player_round");

            migrationBuilder.DropTable(
                name: "admin_log");

            migrationBuilder.DropTable(
                name: "round");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_player_user_id",
                table: "player");
        }
    }
}