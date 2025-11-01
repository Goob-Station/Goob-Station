using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
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
                    book_printer_entry_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(type: "TEXT", nullable: false),
                    author = table.Column<string>(type: "TEXT", nullable: false),
                    genre = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    content = table.Column<string>(type: "TEXT", nullable: false),
                    binding_maps = table.Column<string>(type: "TEXT", nullable: false),
                    binding_paths = table.Column<string>(type: "TEXT", nullable: false),
                    binding_states = table.Column<string>(type: "TEXT", nullable: false)
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
