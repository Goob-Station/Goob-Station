using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class RecoveryPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recovery_password",
                columns: table => new
                {
                    recovery_password_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    player_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    username_at_creation = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    salt = table.Column<byte[]>(type: "BLOB", nullable: false),
                    hash = table.Column<byte[]>(type: "BLOB", nullable: false),
                    iterations = table.Column<int>(type: "INTEGER", nullable: false),
                    algo_version = table.Column<int>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recovery_password", x => x.recovery_password_id);
                    table.ForeignKey(
                        name: "FK_recovery_password_player_player_user_id",
                        column: x => x.player_user_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_recovery_password_player_user_id",
                table: "recovery_password",
                column: "player_user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recovery_password");
        }
    }
}
