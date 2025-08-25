using BouncerBot.Db;
using BouncerBot.Db.Models;
using BouncerBot.Modules.WhoIs;

using Microsoft.EntityFrameworkCore;

namespace BouncerBot.Tests.Modules.WhoIs;

[TestClass]
public class WhoIsServiceTests
{
    private BouncerBotDbContext _dbContext = null!;
    private WhoIsService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<BouncerBotDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new BouncerBotDbContext(options);
        _service = new WhoIsService(_dbContext);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task GetVerifiedUserByDiscordIdAsync_WhenUserExists_ReturnsUser()
    {
        // Arrange
        const ulong guildId = 123456789;
        const ulong discordId = 987654321;
        const uint mouseHuntId = 12345;

        var verifiedUser = new VerifiedUser
        {
            GuildId = guildId,
            DiscordId = discordId,
            MouseHuntId = mouseHuntId
        };

        _dbContext.VerifiedUsers.Add(verifiedUser);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetVerifiedUserByDiscordIdAsync(guildId, discordId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(guildId, result.GuildId);
        Assert.AreEqual(discordId, result.DiscordId);
        Assert.AreEqual(mouseHuntId, result.MouseHuntId);
    }

    [TestMethod]
    public async Task GetVerifiedUserByDiscordIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        const ulong guildId = 123456789;
        const ulong discordId = 987654321;

        // Act
        var result = await _service.GetVerifiedUserByDiscordIdAsync(guildId, discordId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetVerifiedUserByMouseHuntIdAsync_WhenHunterExists_ReturnsUser()
    {
        // Arrange
        const ulong guildId = 123456789;
        const ulong discordId = 987654321;
        const uint mouseHuntId = 12345;

        var verifiedUser = new VerifiedUser
        {
            GuildId = guildId,
            DiscordId = discordId,
            MouseHuntId = mouseHuntId
        };

        _dbContext.VerifiedUsers.Add(verifiedUser);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetVerifiedUserByMouseHuntIdAsync(guildId, mouseHuntId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(guildId, result.GuildId);
        Assert.AreEqual(discordId, result.DiscordId);
        Assert.AreEqual(mouseHuntId, result.MouseHuntId);
    }

    [TestMethod]
    public async Task GetVerifiedUserByMouseHuntIdAsync_WhenHunterDoesNotExist_ReturnsNull()
    {
        // Arrange
        const ulong guildId = 123456789;
        const uint mouseHuntId = 12345;

        // Act
        var result = await _service.GetVerifiedUserByMouseHuntIdAsync(guildId, mouseHuntId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetVerifiedUserByDiscordIdAsync_WithMultipleGuilds_ReturnsCorrectGuildUser()
    {
        // Arrange
        const ulong guildId1 = 123456789;
        const ulong guildId2 = 987654321;
        const ulong discordId = 555666777;
        const uint mouseHuntId1 = 12345;
        const uint mouseHuntId2 = 67890;

        var verifiedUser1 = new VerifiedUser
        {
            GuildId = guildId1,
            DiscordId = discordId,
            MouseHuntId = mouseHuntId1
        };

        var verifiedUser2 = new VerifiedUser
        {
            GuildId = guildId2,
            DiscordId = discordId,
            MouseHuntId = mouseHuntId2
        };

        _dbContext.VerifiedUsers.AddRange(verifiedUser1, verifiedUser2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result1 = await _service.GetVerifiedUserByDiscordIdAsync(guildId1, discordId);
        var result2 = await _service.GetVerifiedUserByDiscordIdAsync(guildId2, discordId);

        // Assert
        Assert.IsNotNull(result1);
        Assert.AreEqual(mouseHuntId1, result1.MouseHuntId);
        
        Assert.IsNotNull(result2);
        Assert.AreEqual(mouseHuntId2, result2.MouseHuntId);
    }
}
