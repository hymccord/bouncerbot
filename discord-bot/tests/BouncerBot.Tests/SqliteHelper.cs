using BouncerBot.Db;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BouncerBot.Tests;
internal static class SqliteHelper
{
    public static BouncerBotDbContext CreateInMemoryDatabase()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<BouncerBotDbContext>()
            .UseSqlite(connection, contextOwnsConnection: true)
            .Options;

        var dbContext = new BouncerBotDbContext(options);
        dbContext.Database.EnsureCreated();

        return dbContext;
    }
}
