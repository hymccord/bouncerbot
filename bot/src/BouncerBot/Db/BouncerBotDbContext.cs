using BouncerBot.Db.Models;

using Microsoft.EntityFrameworkCore;

namespace BouncerBot.Db;
public class BouncerBotDbContext(DbContextOptions<BouncerBotDbContext> options) : DbContext(options)
{
    public DbSet<AchievementMessage> AchievementMessages { get; set; } = null!;
    public DbSet<LogSetting> LogSettings { get; set; } = null!;
    public DbSet<RoleSetting> RoleSettings { get; set; } = null!;
    public DbSet<Snuid> SnuidCache { get; set; } = null!;
    public DbSet<VerifySetting> VerifySettings { get; set; } = null!;
    public DbSet<VerifiedUser> VerifiedUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AchievementMessage>(am =>
        {
            am.HasKey(am => am.GuildId);
        });

        modelBuilder.Entity<LogSetting>(ls =>
        {
            ls.HasKey(l => l.GuildId);
        });

        modelBuilder.Entity<VerifySetting>(rs =>
        {
            rs.HasKey(r => r.GuildId);
        });

        modelBuilder.Entity<RoleSetting>(ar =>
        {
            ar.HasKey(am => am.GuildId);
        });

        modelBuilder.Entity<Snuid>(snuid =>
        {
            snuid.HasKey(s => s.MouseHuntId);
            snuid.Property(s => s.MouseHuntId).ValueGeneratedNever();
        });

        modelBuilder.Entity<VerifiedUser>(vu =>
        {
            vu.HasKey(v => new { v.MouseHuntId, v.GuildId, v.DiscordId });

            vu.HasIndex(vu => new
            {
                vu.GuildId,
                vu.DiscordId
            }).IsUnique(true);

            vu.HasIndex(vu => new
            {
                vu.GuildId,
                vu.MouseHuntId
            }).IsUnique(true);
        });
    }
}
