using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class Books : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "book_printer_entry",
                columns: table => new
                {
                    book_printer_entry_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    author = table.Column<string>(type: "text", nullable: false),
                    genre = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<List<string>>(type: "text[]", nullable: false),
                    binding_maps = table.Column<List<string>>(type: "text[]", nullable: false),
                    binding_paths = table.Column<List<string>>(type: "text[]", nullable: false),
                    binding_states = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_book_printer_entry", x => x.book_printer_entry_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_book_printer_entry_book_printer_entry_id",
                table: "book_printer_entry",
                column: "book_printer_entry_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "book_printer_entry");
        }
    }
}
