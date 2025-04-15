// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore.Migrations;

namespace Content.Server.Database.Migrations.Postgres
{
    public partial class HWID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "HaveEitherAddressOrUserId",
                table: "server_ban");

            migrationBuilder.AddColumn<byte[]>(
                name: "hwid",
                table: "server_ban",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "last_seen_hwid",
                table: "player",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "hwid",
                table: "connection_log",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "HaveEitherAddressOrUserIdOrHWId",
                table: "server_ban",
                sql: "address IS NOT NULL OR user_id IS NOT NULL OR hwid IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "HaveEitherAddressOrUserIdOrHWId",
                table: "server_ban");

            migrationBuilder.DropColumn(
                name: "hwid",
                table: "server_ban");

            migrationBuilder.DropColumn(
                name: "last_seen_hwid",
                table: "player");

            migrationBuilder.DropColumn(
                name: "hwid",
                table: "connection_log");

            migrationBuilder.AddCheckConstraint(
                name: "HaveEitherAddressOrUserId",
                table: "server_ban",
                sql: "address IS NOT NULL OR user_id IS NOT NULL");
        }
    }
}