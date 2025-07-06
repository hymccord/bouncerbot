using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonstroBot.Db.Migrations
{
    /// <inheritdoc />
    public partial class AddVerifiedTradeMapBanned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievementRoles");

            migrationBuilder.CreateTable(
                name: "RoleSettings",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StarId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    CrownId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    CheckmarkId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    EggMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    VerifiedId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    TradeBannedId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    MapBannedId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    ArcaneMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    DraconicMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    ForgottenMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    HydroMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    LawMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    PhysicalMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    RiftMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    ShadowMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    TacticalMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    MultiMasterId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleSettings", x => x.GuildId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleSettings");

            migrationBuilder.CreateTable(
                name: "AchievementRoles",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArcaneMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    CheckmarkId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    CrownId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    DraconicMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    EggMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    ForgottenMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    HyderoMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    LawMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    MultiMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    PhysicalMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    RiftMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    ShadowMasterId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    StarId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    TacticalMasterId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementRoles", x => x.GuildId);
                });
        }
    }
}
