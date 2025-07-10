using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BouncerBot.Db.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VerifiedUsers",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DiscordId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    MouseHuntId = table.Column<uint>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerifiedUsers", x => new { x.MouseHuntId, x.GuildId, x.DiscordId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_VerifiedUsers_GuildId_DiscordId",
                table: "VerifiedUsers",
                columns: new[] { "GuildId", "DiscordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VerifiedUsers_GuildId_MouseHuntId",
                table: "VerifiedUsers",
                columns: new[] { "GuildId", "MouseHuntId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VerifiedUsers");
        }
    }
}
