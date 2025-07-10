using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BouncerBot.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AchievementMessages",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Star = table.Column<string>(type: "TEXT", nullable: true),
                    Crown = table.Column<string>(type: "TEXT", nullable: true),
                    Checkmark = table.Column<string>(type: "TEXT", nullable: true),
                    EggMaster = table.Column<string>(type: "TEXT", nullable: true),
                    ArcaneMaster = table.Column<string>(type: "TEXT", nullable: true),
                    DraconicMaster = table.Column<string>(type: "TEXT", nullable: true),
                    ForgottenMaster = table.Column<string>(type: "TEXT", nullable: true),
                    HyderoMaster = table.Column<string>(type: "TEXT", nullable: true),
                    LawMaster = table.Column<string>(type: "TEXT", nullable: true),
                    PhysicalMaster = table.Column<string>(type: "TEXT", nullable: true),
                    RiftMaster = table.Column<string>(type: "TEXT", nullable: true),
                    ShadowMaster = table.Column<string>(type: "TEXT", nullable: true),
                    TacticalMaster = table.Column<string>(type: "TEXT", nullable: true),
                    MultiMaster = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementMessages", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "AchievementRoles",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StarId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    CrownId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    CheckmarkId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    EggMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    ArcaneMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    DraconicMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    ForgottenMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    HyderoMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    LawMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    PhysicalMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    RiftMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    ShadowMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    TacticalMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    MultiMasterId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementRoles", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "LogSettings",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VerificationId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    FlexId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    EggMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    LogId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogSettings", x => x.GuildId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievementMessages");

            migrationBuilder.DropTable(
                name: "AchievementRoles");

            migrationBuilder.DropTable(
                name: "LogSettings");
        }
    }
}
