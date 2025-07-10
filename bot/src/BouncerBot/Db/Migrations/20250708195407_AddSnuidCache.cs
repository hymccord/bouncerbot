using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BouncerBot.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddSnuidCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SnuidCache",
                columns: table => new
                {
                    MouseHuntId = table.Column<uint>(type: "INTEGER", nullable: false),
                    SnuId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnuidCache", x => x.MouseHuntId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SnuidCache");
        }
    }
}
