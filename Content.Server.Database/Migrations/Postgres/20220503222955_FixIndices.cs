// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    public partial class FixIndices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_admin_log_message",
                table: "admin_log");

            migrationBuilder.CreateIndex(
                name: "IX_admin_log_date",
                table: "admin_log",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_admin_log_message",
                table: "admin_log",
                column: "message")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:TsVectorConfig", "english");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_admin_log_date",
                table: "admin_log");

            migrationBuilder.DropIndex(
                name: "IX_admin_log_message",
                table: "admin_log");

            migrationBuilder.CreateIndex(
                name: "IX_admin_log_message",
                table: "admin_log",
                column: "message")
                .Annotation("Npgsql:TsVectorConfig", "english");
        }
    }
}