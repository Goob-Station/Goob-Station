// SPDX-FileCopyrightText: 2020 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Content.Server.Database.Migrations.Postgres
{
    [DbContext(typeof(PostgresServerDbContext))]
    [Migration("20201006223000_SelectedCharacterSlotFk")]
    public class SelectedCharacterSlotFk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"ALTER TABLE preference
ADD CONSTRAINT ""FK_preference_profile_selected_character_slot_preference_id""
FOREIGN KEY (selected_character_slot, preference_id)
REFERENCES profile (slot, preference_id)
DEFERRABLE INITIALLY DEFERRED;");
        }
    }
}