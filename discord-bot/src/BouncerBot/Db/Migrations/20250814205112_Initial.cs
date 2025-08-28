using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable

namespace BouncerBot.Db.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AchievementLogOverrides",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    AchievementRole = table.Column<uint>(type: "INTEGER", nullable: false),
                    ChannelId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementLogOverrides", x => new { x.GuildId, x.AchievementRole });
                });

            migrationBuilder.CreateTable(
                name: "AchievementMessages",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AchievementRole = table.Column<uint>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementMessages", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "BannedHunters",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    MouseHuntId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannedHunters", x => new { x.MouseHuntId, x.GuildId });
                });

            migrationBuilder.CreateTable(
                name: "LogSettings",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GeneralId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    AchievementId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    VerificationId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogSettings", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "RoleSettings",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscordRoleId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleSettings", x => new { x.GuildId, x.Role });
                });

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

            migrationBuilder.CreateTable(
                name: "VerificationHistory",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DiscordIdHash = table.Column<string>(type: "TEXT", nullable: false),
                    MouseHuntIdHash = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationHistory", x => new { x.GuildId, x.DiscordIdHash });
                });

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

            migrationBuilder.CreateTable(
                name: "VerifySettings",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MinimumRank = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerifySettings", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "VerifiedUsers",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DiscordId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    MouseHuntId = table.Column<uint>(type: "INTEGER", nullable: false),
                    VerifyMessageId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerifiedUsers", x => new { x.GuildId, x.DiscordId });
                    table.ForeignKey(
                        name: "FK_VerifiedUsers_VerifyMessages_VerifyMessageId",
                        column: x => x.VerifyMessageId,
                        principalTable: "VerifyMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_VerificationHistory_GuildId_MouseHuntIdHash",
                table: "VerificationHistory",
                columns: new[] { "GuildId", "MouseHuntIdHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VerifiedUsers_GuildId_MouseHuntId",
                table: "VerifiedUsers",
                columns: new[] { "GuildId", "MouseHuntId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VerifiedUsers_VerifyMessageId",
                table: "VerifiedUsers",
                column: "VerifyMessageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievementLogOverrides");

            migrationBuilder.DropTable(
                name: "AchievementMessages");

            migrationBuilder.DropTable(
                name: "BannedHunters");

            migrationBuilder.DropTable(
                name: "LogSettings");

            migrationBuilder.DropTable(
                name: "RoleSettings");

            migrationBuilder.DropTable(
                name: "SnuidCache");

            migrationBuilder.DropTable(
                name: "VerificationHistory");

            migrationBuilder.DropTable(
                name: "VerifiedUsers");

            migrationBuilder.DropTable(
                name: "VerifySettings");

            migrationBuilder.DropTable(
                name: "VerifyMessages");
        }
    }
}
