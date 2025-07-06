using Microsoft.EntityFrameworkCore;

using MonstroBot.Db.Models;

namespace MonstroBot.Db;
public class MonstroBotDbContext(DbContextOptions<MonstroBotDbContext> options) : DbContext(options)
{
    public DbSet<AchievementMessage> AchievementMessages { get; set; } = null!;
    public DbSet<RoleSetting> RoleSettings { get; set; } = null!;
    public DbSet<LogSetting> LogSettings { get; set; } = null!;
    public DbSet<VerifiedUser> VerifiedUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AchievementMessage>(am =>
        {
            am.HasKey(am => am.GuildId);
        });

        modelBuilder.Entity<RoleSetting>(ar =>
        {
            ar.HasKey(am => am.GuildId);
        });

        modelBuilder.Entity<LogSetting>(ls =>
        {
            ls.HasKey(l => l.GuildId);
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

//public class MonstroBotDbContextFactory : IDesignTimeDbContextFactory<MonstroBotDbContext>
//{
//    public MonstroBotDbContext CreateDbContext(string[] args)
//    {
//        var optionsBuilder = new DbContextOptionsBuilder<MonstroBotDbContext>();
//        optionsBuilder.UseSqlite("");
//        return new MonstroBotDbContext(optionsBuilder.Options);
//    }
//}
