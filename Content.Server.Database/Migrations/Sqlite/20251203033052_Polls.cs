using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class Polls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "polls",
                columns: table => new
                {
                    polls_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    start_time = table.Column<DateTime>(type: "TEXT", nullable: false),
                    end_time = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_by_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    active = table.Column<bool>(type: "INTEGER", nullable: false),
                    allow_multiple_choices = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_polls", x => x.polls_id);
                    table.ForeignKey(
                        name: "FK_polls_player_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "poll_options",
                columns: table => new
                {
                    poll_options_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    poll_id = table.Column<int>(type: "INTEGER", nullable: false),
                    option_text = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    display_order = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poll_options", x => x.poll_options_id);
                    table.ForeignKey(
                        name: "FK_poll_options_polls_poll_id",
                        column: x => x.poll_id,
                        principalTable: "polls",
                        principalColumn: "polls_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "poll_votes",
                columns: table => new
                {
                    poll_votes_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    poll_id = table.Column<int>(type: "INTEGER", nullable: false),
                    poll_option_id = table.Column<int>(type: "INTEGER", nullable: false),
                    player_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    voted_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poll_votes", x => x.poll_votes_id);
                    table.ForeignKey(
                        name: "FK_poll_votes_player_player_user_id",
                        column: x => x.player_user_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_poll_votes_poll_options_poll_option_id",
                        column: x => x.poll_option_id,
                        principalTable: "poll_options",
                        principalColumn: "poll_options_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_poll_votes_polls_poll_id",
                        column: x => x.poll_id,
                        principalTable: "polls",
                        principalColumn: "polls_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_poll_options_poll_id",
                table: "poll_options",
                column: "poll_id");

            migrationBuilder.CreateIndex(
                name: "IX_poll_votes_player_user_id",
                table: "poll_votes",
                column: "player_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_poll_votes_poll_id",
                table: "poll_votes",
                column: "poll_id");

            migrationBuilder.CreateIndex(
                name: "IX_poll_votes_poll_id_player_user_id_poll_option_id",
                table: "poll_votes",
                columns: new[] { "poll_id", "player_user_id", "poll_option_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_poll_votes_poll_option_id",
                table: "poll_votes",
                column: "poll_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_polls_active",
                table: "polls",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_polls_created_by_id",
                table: "polls",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_polls_end_time",
                table: "polls",
                column: "end_time");

            migrationBuilder.CreateIndex(
                name: "IX_polls_start_time",
                table: "polls",
                column: "start_time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "poll_votes");

            migrationBuilder.DropTable(
                name: "poll_options");

            migrationBuilder.DropTable(
                name: "polls");
        }
    }
}
