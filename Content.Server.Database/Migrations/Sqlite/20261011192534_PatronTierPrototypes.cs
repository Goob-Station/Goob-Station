using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class PatronTierPrototypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ooc_icon",
                table: "rmc_patrons",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "rmc_patron_tiers_id",
                table: "rmc_patron_tiers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ooc_icon",
                table: "rmc_patrons");

            migrationBuilder.AlterColumn<int>(
                name: "rmc_patron_tiers_id",
                table: "rmc_patron_tiers",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);
        }
    }
}
