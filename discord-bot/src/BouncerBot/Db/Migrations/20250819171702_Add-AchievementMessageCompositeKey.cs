using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable

namespace BouncerBot.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddAchievementMessageCompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AchievementMessages",
                table: "AchievementMessages");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "AchievementMessages",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AchievementMessages",
                table: "AchievementMessages",
                columns: new[] { "GuildId", "AchievementRole" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AchievementMessages",
                table: "AchievementMessages");

            migrationBuilder.AlterColumn<ulong>(
                name: "GuildId",
                table: "AchievementMessages",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AchievementMessages",
                table: "AchievementMessages",
                column: "GuildId");
        }
    }
}
