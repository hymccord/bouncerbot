using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BouncerBot.Db.Migrations
{
    /// <inheritdoc />
    public partial class ConvertEnumDataToStrings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert MinimumRank data
            migrationBuilder.Sql(@"
                UPDATE VerifySettings 
                SET MinimumRank = CASE CAST(MinimumRank AS INTEGER)
                    WHEN 0 THEN 'Novice'
                    WHEN 1 THEN 'Recruit'
                    WHEN 2 THEN 'Apprentice'
                    WHEN 3 THEN 'Initiate'
                    WHEN 4 THEN 'Journeyman'
                    WHEN 5 THEN 'Master'
                    WHEN 6 THEN 'Grandmaster'
                    WHEN 7 THEN 'Legendary'
                    WHEN 8 THEN 'Hero'
                    WHEN 9 THEN 'Knight'
                    WHEN 10 THEN 'Lord'
                    WHEN 11 THEN 'Baron'
                    WHEN 12 THEN 'Count'
                    WHEN 13 THEN 'Duke'
                    WHEN 14 THEN 'GrandDuke'
                    WHEN 15 THEN 'Archduke'
                    WHEN 16 THEN 'Viceroy'
                    WHEN 17 THEN 'Elder'
                    WHEN 18 THEN 'Sage'
                    WHEN 19 THEN 'Fabled'
                    ELSE MinimumRank
                END
                WHERE MinimumRank GLOB '[0-9]*'");

            // Convert Role data
            migrationBuilder.Sql(@"
                UPDATE RoleSettings 
                SET Role = CASE CAST(Role AS INTEGER)
                    WHEN 0 THEN 'Verified'
                    WHEN 1 THEN 'Star'
                    WHEN 2 THEN 'Crown'
                    WHEN 3 THEN 'Checkmark'
                    WHEN 4 THEN 'EggMaster'
                    WHEN 5 THEN 'Achiever'
                    WHEN 6 THEN 'ArcaneMaster'
                    WHEN 7 THEN 'DraconicMaster'
                    WHEN 8 THEN 'ForgottenMaster'
                    WHEN 9 THEN 'HydroMaster'
                    WHEN 10 THEN 'LawMaster'
                    WHEN 11 THEN 'PhysicalMaster'
                    WHEN 12 THEN 'RiftMaster'
                    WHEN 13 THEN 'ShadowMaster'
                    WHEN 14 THEN 'TacticalMaster'
                    WHEN 15 THEN 'MultiMaster'
                    ELSE Role
                END
                WHERE Role GLOB '[0-9]*'");

            // Convert AchievementRole data
            migrationBuilder.Sql(@"
                UPDATE AchievementMessages 
                SET AchievementRole = CASE CAST(AchievementRole AS INTEGER)
                    WHEN 1 THEN 'Star'
                    WHEN 2 THEN 'Crown'
                    WHEN 3 THEN 'Checkmark'
                    WHEN 4 THEN 'EggMaster'
                    WHEN 11 THEN 'ArcaneMaster'
                    WHEN 12 THEN 'DraconicMaster'
                    WHEN 13 THEN 'ForgottenMaster'
                    WHEN 14 THEN 'HydroMaster'
                    WHEN 15 THEN 'LawMaster'
                    WHEN 16 THEN 'PhysicalMaster'
                    WHEN 17 THEN 'RiftMaster'
                    WHEN 18 THEN 'ShadowMaster'
                    WHEN 19 THEN 'TacticalMaster'
                    WHEN 20 THEN 'MultiMaster'
                    ELSE AchievementRole
                END
                WHERE AchievementRole GLOB '[0-9]*'");

            migrationBuilder.Sql(@"
                UPDATE AchievementLogOverrides 
                SET AchievementRole = CASE CAST(AchievementRole AS INTEGER)
                    WHEN 1 THEN 'Star'
                    WHEN 2 THEN 'Crown'
                    WHEN 3 THEN 'Checkmark'
                    WHEN 4 THEN 'EggMaster'
                    WHEN 11 THEN 'ArcaneMaster'
                    WHEN 12 THEN 'DraconicMaster'
                    WHEN 13 THEN 'ForgottenMaster'
                    WHEN 14 THEN 'HydroMaster'
                    WHEN 15 THEN 'LawMaster'
                    WHEN 16 THEN 'PhysicalMaster'
                    WHEN 17 THEN 'RiftMaster'
                    WHEN 18 THEN 'ShadowMaster'
                    WHEN 19 THEN 'TacticalMaster'
                    WHEN 20 THEN 'MultiMaster'
                    ELSE AchievementRole
                END
                WHERE AchievementRole GLOB '[0-9]*'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert MinimumRank data
            migrationBuilder.Sql(@"
                UPDATE VerifySettings 
                SET MinimumRank = CASE CAST(MinimumRank AS INTEGER)
                    WHEN 'Novice' THEN '0'
                    WHEN 'Recruit' THEN '1'
                    WHEN 'Apprentice' THEN '2'
                    WHEN 'Initiate' THEN '3'
                    WHEN 'Journeyman' THEN '4'
                    WHEN 'Master' THEN '5'
                    WHEN 'Grandmaster' THEN '6'
                    WHEN 'Legendary' THEN '7'
                    WHEN 'Hero' THEN '8'
                    WHEN 'Knight' THEN '9'
                    WHEN 'Lord' THEN '10'
                    WHEN 'Baron' THEN '11'
                    WHEN 'Count' THEN '12'
                    WHEN 'Duke' THEN '13'
                    WHEN 'GrandDuke' THEN '14'
                    WHEN 'Archduke' THEN '15'
                    WHEN 'Viceroy' THEN '16'
                    WHEN 'Elder' THEN '17'
                    WHEN 'Sage' THEN '18'
                    WHEN 'Fabled' THEN '19'
                    ELSE MinimumRank
                END
                WHERE MinimumRank GLOB '*[a-zA-Z]*'");

            // Convert Role data
            migrationBuilder.Sql(@"
                UPDATE RoleSettings 
                SET Role = CASE CAST(Role AS INTEGER)
                    WHEN 'Verified' THEN '0'
                    WHEN 'Star' THEN '1'
                    WHEN 'Crown' THEN '2'
                    WHEN 'Checkmark' THEN '3'
                    WHEN 'EggMaster' THEN '4'
                    WHEN 'Achiever' THEN '5'
                    WHEN 'ArcaneMaster' THEN '6'
                    WHEN 'DraconicMaster' THEN '7'
                    WHEN 'ForgottenMaster' THEN '8'
                    WHEN 'HydroMaster' THEN '9'
                    WHEN 'LawMaster' THEN '10'
                    WHEN 'PhysicalMaster' THEN '11'
                    WHEN 'RiftMaster' THEN '12'
                    WHEN 'ShadowMaster' THEN '13'
                    WHEN 'TacticalMaster' THEN '14'
                    WHEN 'MultiMaster' THEN '15'
                    ELSE Role
                END
                WHERE Role GLOB '*[a-zA-Z]*'");

            // Convert AchievementRole data
            migrationBuilder.Sql(@"
                UPDATE AchievementMessages 
                SET AchievementRole = CASE CAST(AchievementRole AS INTEGER)
                    WHEN 'Star' THEN '1'
                    WHEN 'Crown' THEN '2'
                    WHEN 'Checkmark' THEN '3'
                    WHEN 'EggMaster' THEN '4'
                    WHEN 'ArcaneMaster' THEN '11'
                    WHEN 'DraconicMaster' THEN '12'
                    WHEN 'ForgottenMaster' THEN '13'
                    WHEN 'HydroMaster' THEN '14'
                    WHEN 'LawMaster' THEN '15'
                    WHEN 'PhysicalMaster' THEN '16'
                    WHEN 'RiftMaster' THEN '17'
                    WHEN 'ShadowMaster' THEN '18'
                    WHEN 'TacticalMaster' THEN '19'
                    WHEN 'MultiMaster' THEN '20'
                    ELSE AchievementRole
                END
                WHERE AchievementRole GLOB '*[a-zA-Z]*'");

            migrationBuilder.Sql(@"
                UPDATE AchievementLogOverrides 
                SET AchievementRole = CASE CAST(AchievementRole AS INTEGER)
                    WHEN 'Star' THEN '1'
                    WHEN 'Crown' THEN '2'
                    WHEN 'Checkmark' THEN '3'
                    WHEN 'EggMaster' THEN '4'
                    WHEN 'ArcaneMaster' THEN '11'
                    WHEN 'DraconicMaster' THEN '12'
                    WHEN 'ForgottenMaster' THEN '13'
                    WHEN 'HydroMaster' THEN '14'
                    WHEN 'LawMaster' THEN '15'
                    WHEN 'PhysicalMaster' THEN '16'
                    WHEN 'RiftMaster' THEN '17'
                    WHEN 'ShadowMaster' THEN '18'
                    WHEN 'TacticalMaster' THEN '19'
                    WHEN 'MultiMaster' THEN '20'
                    ELSE AchievementRole
                END
                WHERE AchievementRole GLOB '*[a-zA-Z]*'");
        }
    }
}
