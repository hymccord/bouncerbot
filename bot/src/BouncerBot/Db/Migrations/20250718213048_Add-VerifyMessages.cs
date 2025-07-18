using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BouncerBot.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddVerifyMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VerifyMessageId",
                table: "VerifiedUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VerifyMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MessageId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerifyMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VerifiedUsers_VerifyMessageId",
                table: "VerifiedUsers",
                column: "VerifyMessageId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VerifiedUsers_VerifyMessages_VerifyMessageId",
                table: "VerifiedUsers",
                column: "VerifyMessageId",
                principalTable: "VerifyMessages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VerifiedUsers_VerifyMessages_VerifyMessageId",
                table: "VerifiedUsers");

            migrationBuilder.DropTable(
                name: "VerifyMessages");

            migrationBuilder.DropIndex(
                name: "IX_VerifiedUsers_VerifyMessageId",
                table: "VerifiedUsers");

            migrationBuilder.DropColumn(
                name: "VerifyMessageId",
                table: "VerifiedUsers");
        }
    }
}
