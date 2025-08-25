using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BouncerBot.Db.Migrations
{
    /// <inheritdoc />
    public partial class StoreEnumAsString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MinimumRank",
                table: "VerifySettings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "RoleSettings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "AchievementRole",
                table: "AchievementMessages",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "AchievementRole",
                table: "AchievementLogOverrides",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MinimumRank",
                table: "VerifySettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "RoleSettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<uint>(
                name: "AchievementRole",
                table: "AchievementMessages",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<uint>(
                name: "AchievementRole",
                table: "AchievementLogOverrides",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
