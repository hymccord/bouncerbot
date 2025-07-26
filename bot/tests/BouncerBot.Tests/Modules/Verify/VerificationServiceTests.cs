using BouncerBot.Db;
using BouncerBot.Db.Models;
using BouncerBot.Modules.Verify;
using BouncerBot.Rest;
using BouncerBot.Rest.Models;
using BouncerBot.Services;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

using NetCord.Rest;

using NSubstitute;

namespace BouncerBot.Tests.Modules.Verify;

[TestClass]
public class VerificationServiceTests
{
    private FakeLogger<VerificationService> _logger = null!;
    private IRestRequestHandler _restRequestHandler = null!;
    private BouncerBotDbContext _dbContext = null!;
    private IDiscordRestClient _restClient = null!;
    private IMouseHuntRestClient _mouseHuntRestClient = null!;
    private SqliteConnection _connection;
    private VerificationService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _logger = new FakeLogger<VerificationService>();
        _restRequestHandler = Substitute.For<IRestRequestHandler>();
        _restClient = Substitute.For<IDiscordRestClient>();
        _mouseHuntRestClient = Substitute.For<IMouseHuntRestClient>();

        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<BouncerBotDbContext>()
            .UseSqlite(_connection)
            .Options;
        _dbContext = new BouncerBotDbContext(options);
        _dbContext.Database.EnsureCreated();

        _service = new VerificationService(_logger, _dbContext, _restClient, _mouseHuntRestClient);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _connection.Dispose();
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task AddVerifiedUserAsync_NewUser_AddsUserAndReturnsTrue()
    {
        // Arrange
        uint mouseHuntId = 12345;
        ulong guildId = 67890;
        ulong discordId = 11111;
        ulong roleId = 22222;

        _dbContext.RoleSettings.Add(new RoleSetting { GuildId = guildId, VerifiedId = roleId });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.AddVerifiedUserAsync(mouseHuntId, guildId, discordId);

        // Assert
        Assert.IsTrue(result.WasAdded);
        Assert.AreEqual(mouseHuntId, result.MouseHuntId);

        var verifiedUser = await _dbContext.VerifiedUsers.FirstOrDefaultAsync();
        Assert.IsNotNull(verifiedUser);
        Assert.AreEqual(mouseHuntId, verifiedUser.MouseHuntId);
        Assert.AreEqual(guildId, verifiedUser.GuildId);
        Assert.AreEqual(discordId, verifiedUser.DiscordId);

        var history = await _dbContext.VerificationHistory.FirstOrDefaultAsync();
        Assert.IsNotNull(history);
        Assert.AreEqual(guildId, history.GuildId);
        Assert.AreEqual(discordId, history.DiscordId);

        await _restClient.Received(1).AddGuildUserRoleAsync(guildId, discordId, roleId, cancellationToken: Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task AddVerifiedUserAsync_ExistingUser_DoesNotAddAndReturnsFalse()
    {
        // Arrange
        uint mouseHuntId = 12345;
        ulong guildId = 67890;
        ulong discordId = 11111;

        _dbContext.VerifiedUsers.Add(new VerifiedUser
        {
            MouseHuntId = mouseHuntId,
            GuildId = guildId,
            DiscordId = discordId
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.AddVerifiedUserAsync(mouseHuntId, guildId, discordId);

        // Assert
        Assert.IsFalse(result.WasAdded);
        Assert.AreEqual(mouseHuntId, result.MouseHuntId);
    }

    [TestMethod]
    public async Task AddVerifiedUserAsync_ExistingHistoryWithDifferentHash_ThrowsException()
    {
        // Arrange
        uint mouseHuntId = 12345;
        uint differentMouseHuntId = 54321;
        ulong guildId = 67890;
        ulong discordId = 11111;

        _dbContext.VerificationHistory.Add(new VerificationHistory
        {
            GuildId = guildId,
            DiscordId = discordId,
            MouseHuntIdHash = VerificationHistory.HashMouseHuntId(differentMouseHuntId)
        });
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => _service.AddVerifiedUserAsync(mouseHuntId, guildId, discordId));

    }

    [TestMethod]
    public async Task IsDiscordUserVerifiedAsync_UserExists_ReturnsTrue()
    {
        // Arrange
        ulong guildId = 67890;
        ulong discordId = 11111;

        _dbContext.VerifiedUsers.Add(new VerifiedUser
        {
            MouseHuntId = 12345,
            GuildId = guildId,
            DiscordId = discordId
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.IsDiscordUserVerifiedAsync(guildId, discordId);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsDiscordUserVerifiedAsync_UserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        ulong guildId = 67890;
        ulong discordId = 11111;

        // Act
        var result = await _service.IsDiscordUserVerifiedAsync(guildId, discordId);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CanUserVerifyAsync_MouseHuntIdInUse_ReturnsFalse()
    {
        // Arrange
        uint mouseHuntId = 12345;
        ulong guildId = 67890;
        ulong discordId = 11111;
        ulong otherDiscordId = 22222;

        _dbContext.VerifiedUsers.Add(new VerifiedUser
        {
            MouseHuntId = mouseHuntId,
            GuildId = guildId,
            DiscordId = otherDiscordId
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CanUserVerifyAsync(mouseHuntId, guildId, discordId);

        // Assert
        Assert.IsFalse(result.CanVerify);
        Assert.IsTrue(result.Message.Contains("already linked"));
    }

    [TestMethod]
    public async Task CanUserVerifyAsync_DifferentMouseHuntIdInHistory_ReturnsFalse()
    {
        // Arrange
        uint mouseHuntId = 12345;
        uint differentMouseHuntId = 54321;
        ulong guildId = 67890;
        ulong discordId = 11111;

        _dbContext.VerificationHistory.Add(new VerificationHistory
        {
            GuildId = guildId,
            DiscordId = discordId,
            MouseHuntIdHash = VerificationHistory.HashMouseHuntId(differentMouseHuntId)
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CanUserVerifyAsync(mouseHuntId, guildId, discordId);

        // Assert
        Assert.IsFalse(result.CanVerify);
        Assert.IsTrue(result.Message.Contains("previously used a different MouseHunt ID"));
    }

    [TestMethod]
    public async Task CanUserVerifyAsync_UserRankTooLow_ReturnsFalse()
    {
        // Arrange
        uint mouseHuntId = 12345;
        ulong guildId = 67890;
        ulong discordId = 11111;

        _dbContext.VerifySettings.Add(new VerifySetting
        {
            GuildId = guildId,
            MinimumRank = Rank.Knight
        });
        await _dbContext.SaveChangesAsync();

        var titles = new Title[]
        {
            new() { TitleId = 1, Name = Rank.Novice },
            new() { TitleId = 2, Name = Rank.Knight }
        };

        _mouseHuntRestClient.GetTitlesAsync(Arg.Any<CancellationToken>()).Returns(titles);
        _mouseHuntRestClient.GetUserTitleAsync(mouseHuntId, Arg.Any<CancellationToken>())
            .Returns(new UserTitle { TitleId = 1 });

        // Act
        var result = await _service.CanUserVerifyAsync(mouseHuntId, guildId, discordId);

        // Assert
        Assert.IsFalse(result.CanVerify);
        Assert.IsTrue(result.Message.Contains("Rank up to"));
    }

    [TestMethod]
    public async Task CanUserVerifyAsync_AllChecksPassed_ReturnsTrue()
    {
        // Arrange
        uint mouseHuntId = 12345;
        ulong guildId = 67890;
        ulong discordId = 11111;

        _dbContext.VerifySettings.Add(new VerifySetting
        {
            GuildId = guildId,
            MinimumRank = Rank.Novice
        });
        await _dbContext.SaveChangesAsync();

        var titles = new Title[]
        {
            new() { TitleId = 1, Name = Rank.Knight }
        };

        _mouseHuntRestClient.GetTitlesAsync(Arg.Any<CancellationToken>()).Returns(titles);
        _mouseHuntRestClient.GetUserTitleAsync(mouseHuntId, Arg.Any<CancellationToken>())
            .Returns(new UserTitle { TitleId = 1 });

        // Act
        var result = await _service.CanUserVerifyAsync(mouseHuntId, guildId, discordId);

        // Assert
        Assert.IsTrue(result.CanVerify);
        Assert.AreEqual("", result.Message);
    }

    [TestMethod]
    public async Task RemoveVerifiedUser_UserExists_RemovesUserAndReturnsTrue()
    {
        // Arrange
        uint mouseHuntId = 12345;
        ulong guildId = 67890;
        ulong discordId = 11111;
        ulong roleId = 22222;
        ulong channelId = 33333;
        ulong messageId = 44444;

        var verifyMessage = new VerifyMessage
        {
            ChannelId = channelId,
            MessageId = messageId
        };

        var verifiedUser = new VerifiedUser
        {
            MouseHuntId = mouseHuntId,
            GuildId = guildId,
            DiscordId = discordId,
            VerifyMessage = verifyMessage
        };

        _dbContext.VerifiedUsers.Add(verifiedUser);
        _dbContext.RoleSettings.Add(new RoleSetting { GuildId = guildId, VerifiedId = roleId });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.RemoveVerifiedUser(guildId, discordId);

        // Assert
        Assert.IsTrue(result.WasRemoved);
        Assert.AreEqual(mouseHuntId, result.MouseHuntId);

        var remainingUser = await _dbContext.VerifiedUsers.FirstOrDefaultAsync();
        Assert.IsNull(remainingUser);

        var remainingMessage = await _dbContext.VerifyMessages.FirstOrDefaultAsync();
        Assert.IsNull(remainingMessage);

        await _restClient.Received(1).DeleteMessageAsync(channelId, messageId, cancellationToken: Arg.Any<CancellationToken>());
        await _restClient.Received(1).RemoveGuildUserRoleAsync(guildId, discordId, roleId);
    }

    [TestMethod]
    public async Task RemoveVerifiedUser_UserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        ulong guildId = 67890;
        ulong discordId = 11111;

        // Act
        var result = await _service.RemoveVerifiedUser(guildId, discordId);

        // Assert
        Assert.IsFalse(result.WasRemoved);
        Assert.IsNull(result.MouseHuntId);
    }

    [TestMethod]
    public async Task SetVerificationMessageAsync_UserExists_SetsMessage()
    {
        // Arrange
        uint mouseHuntId = 12345;
        ulong guildId = 67890;
        ulong discordId = 11111;
        ulong channelId = 33333;
        ulong messageId = 44444;

        _dbContext.VerifiedUsers.Add(new VerifiedUser
        {
            MouseHuntId = mouseHuntId,
            GuildId = guildId,
            DiscordId = discordId
        });
        await _dbContext.SaveChangesAsync();

        var parameters = new SetVerificationMessageParameters(guildId, discordId, channelId, messageId);

        // Act
        await _service.SetVerificationMessageAsync(parameters);

        // Assert
        var user = await _dbContext.VerifiedUsers
            .Include(vu => vu.VerifyMessage)
            .FirstOrDefaultAsync();

        Assert.IsNotNull(user?.VerifyMessage);
        Assert.AreEqual(channelId, user.VerifyMessage.ChannelId);
        Assert.AreEqual(messageId, user.VerifyMessage.MessageId);
    }

    [TestMethod]
    public async Task SetVerificationMessageAsync_UserDoesNotExist_LogsWarning()
    {
        // Arrange
        ulong guildId = 67890;
        ulong discordId = 11111;
        ulong channelId = 33333;
        ulong messageId = 44444;

        var parameters = new SetVerificationMessageParameters(guildId, discordId, channelId, messageId);

        // Act
        await _service.SetVerificationMessageAsync(parameters);

        // Assert
        Assert.AreEqual(1, _logger.Collector.Count);
        var r = _logger.LatestRecord;
        Assert.AreEqual(LogLevel.Warning, r.Level);
        Assert.Contains("not verified", r.Message);

        Assert.AreEqual($"{guildId}", r.GetStructuredStateValue("GuildId"));
        Assert.AreEqual($"{discordId}", r.GetStructuredStateValue("UserId"));
        Assert.AreEqual($"{channelId}", r.GetStructuredStateValue("ChannelId"));
        Assert.AreEqual($"{messageId}", r.GetStructuredStateValue("MessageId"));
    }

    [TestMethod]
    public async Task GetUserRankAsync_TitleNotFound_ThrowsException()
    {
        // Arrange
        uint mouseHuntId = 12345;
        ulong guildId = 67890;
        ulong discordId = 11111;

        var titles = new Title[]
        {
            new() { TitleId = 1, Name = Rank.Novice }
        };

        _mouseHuntRestClient.GetTitlesAsync(Arg.Any<CancellationToken>()).Returns(titles);
        _mouseHuntRestClient.GetUserTitleAsync(mouseHuntId, Arg.Any<CancellationToken>())
            .Returns(new UserTitle { TitleId = 999 }); // Non-existent title ID

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => _service.CanUserVerifyAsync(mouseHuntId, guildId, discordId));
    }

    [TestMethod]
    public async Task GetUserTitleAsync_ApiReturnsNull_ThrowsException()
    {
        // Arrange
        uint mouseHuntId = 12345;
        ulong guildId = 67890;
        ulong discordId = 11111;

        _mouseHuntRestClient.GetTitlesAsync(Arg.Any<CancellationToken>()).Returns(new Title[] { });
        _mouseHuntRestClient.GetUserTitleAsync(mouseHuntId, Arg.Any<CancellationToken>())
            .Returns((UserTitle?)null);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.CanUserVerifyAsync(mouseHuntId, guildId, discordId));

        Assert.IsTrue(exception.Message.Contains("Failed to fetch user title"));
    }

    [TestMethod]
    public async Task GetTitlesAsync_ApiReturnsNull_ThrowsException()
    {
        // Arrange
        uint mouseHuntId = 12345;
        ulong guildId = 67890;
        ulong discordId = 11111;

        _mouseHuntRestClient.GetTitlesAsync(Arg.Any<CancellationToken>()).Returns((Title[]?)null);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<Exception>(
            () => _service.CanUserVerifyAsync(mouseHuntId, guildId, discordId));

        Assert.IsTrue(exception.Message.Contains("Failed to fetch titles"));
    }
}
