using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using MonstroBot.Db.Models;

namespace MonstroBot.Db;
public class MonstroBotDbContext(DbContextOptions<MonstroBotDbContext> options) : DbContext(options)
{
    public DbSet<VerifiedUser> VerifiedUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
