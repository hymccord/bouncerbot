using BouncerBot.Db;
using BouncerBot.Db.Models;
using BouncerBot.Modules.WhoIs;

namespace BouncerBot.Tests.Modules.WhoIs;

[TestClass]
public class WhoIsServiceTests
{
    private readonly BouncerBotDbContext _dbContext = null!;
    private readonly WhoIsService _sut = null!;

    public WhoIsServiceTests()
    {
        _dbContext = SqliteHelper.CreateInMemoryDatabase();
        _sut = new WhoIsService(_dbContext);
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
        const ulong GuildId = 123456789;
        const ulong DiscordId = 987654321;
        const uint MouseHuntId = 12345;

        var verifiedUser = new VerifiedUser
        {
            GuildId = GuildId,
            DiscordId = DiscordId,
            MouseHuntId = MouseHuntId
        };

        _dbContext.VerifiedUsers.Add(verifiedUser);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetVerifiedUserByDiscordIdAsync(GuildId, DiscordId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(GuildId, result.GuildId);
        Assert.AreEqual(DiscordId, result.DiscordId);
        Assert.AreEqual(MouseHuntId, result.MouseHuntId);
    }

    [TestMethod]
    public async Task GetVerifiedUserByDiscordIdAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        const ulong GuildId = 123456789;
        const ulong DiscordId = 987654321;

        // Act
        var result = await _sut.GetVerifiedUserByDiscordIdAsync(GuildId, DiscordId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetVerifiedUserByMouseHuntIdAsync_WhenHunterExists_ReturnsUser()
    {
        // Arrange
        const ulong GuildId = 123456789;
        const ulong DiscordId = 987654321;
        const uint MouseHuntId = 12345;

        var verifiedUser = new VerifiedUser
        {
            GuildId = GuildId,
            DiscordId = DiscordId,
            MouseHuntId = MouseHuntId
        };

        _dbContext.VerifiedUsers.Add(verifiedUser);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetVerifiedUserByMouseHuntIdAsync(GuildId, MouseHuntId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(GuildId, result.GuildId);
        Assert.AreEqual(DiscordId, result.DiscordId);
        Assert.AreEqual(MouseHuntId, result.MouseHuntId);
    }

    [TestMethod]
    public async Task GetVerifiedUserByMouseHuntIdAsync_WhenHunterDoesNotExist_ReturnsNull()
    {
        // Arrange
        const ulong GuildId = 123456789;
        const uint MouseHuntId = 12345;

        // Act
        var result = await _sut.GetVerifiedUserByMouseHuntIdAsync(GuildId, MouseHuntId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetVerifiedUserByDiscordIdAsync_WithMultipleGuilds_ReturnsCorrectGuildUser()
    {
        // Arrange
        const ulong GuildId1 = 123456789;
        const ulong GuildId2 = 987654321;
        const ulong DiscordId = 555666777;
        const uint MouseHuntId1 = 12345;
        const uint MouseHuntId2 = 67890;

        var verifiedUser1 = new VerifiedUser
        {
            GuildId = GuildId1,
            DiscordId = DiscordId,
            MouseHuntId = MouseHuntId1
        };

        var verifiedUser2 = new VerifiedUser
        {
            GuildId = GuildId2,
            DiscordId = DiscordId,
            MouseHuntId = MouseHuntId2
        };

        _dbContext.VerifiedUsers.AddRange(verifiedUser1, verifiedUser2);
        await _dbContext.SaveChangesAsync();

        // Act
        var result1 = await _sut.GetVerifiedUserByDiscordIdAsync(GuildId1, DiscordId);
        var result2 = await _sut.GetVerifiedUserByDiscordIdAsync(GuildId2, DiscordId);

        // Assert
        Assert.IsNotNull(result1);
        Assert.AreEqual(MouseHuntId1, result1.MouseHuntId);
        
        Assert.IsNotNull(result2);
        Assert.AreEqual(MouseHuntId2, result2.MouseHuntId);
    }
}
