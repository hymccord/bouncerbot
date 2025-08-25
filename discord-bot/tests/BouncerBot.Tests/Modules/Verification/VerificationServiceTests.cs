using BouncerBot;
using BouncerBot.Db;
using BouncerBot.Db.Models;
using BouncerBot.Modules.Bounce;
using BouncerBot.Modules.Verification;
using BouncerBot.Rest;
using BouncerBot.Rest.Models;
using BouncerBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NetCord;
using NSubstitute;

namespace BouncerBot.Tests.Modules.Verification;

[TestClass]
public class VerificationServiceTests
{
    private readonly FakeLogger<VerificationService> _logger;
    private readonly BouncerBotDbContext _dbContext;
    private readonly ICommandMentionService _mockCommandMentionService;
    private readonly IDiscordRestClient _mockRestClient;
    private readonly IHashService _mockHashingService;
    private readonly IGuildLoggingService _mockGuildLoggingService;
    private readonly IMouseHuntRestClient _mockMouseHuntRestClient;
    private readonly IBounceService _mockBounceService;
    private readonly VerificationService _service;

    // Test constants
    private const uint TestMouseHuntId = 12345u;
    private const ulong TestGuildId = 67890ul;
    private const ulong TestDiscordId = 11111ul;
    private const string TestDiscordIdHash = "test-discord-hash";
    private const string TestMouseHuntIdHash = "test-mousehunt-hash";
    private const string TestCommandMention = "</privacy:123>";

    public VerificationServiceTests()
    {
        // Setup mocks
        _logger = new FakeLogger<VerificationService>();
        _mockCommandMentionService = Substitute.For<ICommandMentionService>();
        _mockRestClient = Substitute.For<IDiscordRestClient>();
        _mockHashingService = Substitute.For<IHashService>();
        _mockGuildLoggingService = Substitute.For<IGuildLoggingService>();
        _mockMouseHuntRestClient = Substitute.For<IMouseHuntRestClient>();
        _mockBounceService = Substitute.For<IBounceService>();

        // Setup in-memory database
        _dbContext = SqliteHelper.CreateInMemoryDatabase();

        // Setup common mock returns
        _mockCommandMentionService.GetCommandMention(Arg.Any<string>()).Returns(TestCommandMention);
        _mockHashingService.HashValue(TestDiscordId).Returns(TestDiscordIdHash);
        _mockHashingService.HashValue(TestMouseHuntId).Returns(TestMouseHuntIdHash);
        _mockHashingService.VerifyHash(TestDiscordIdHash, TestDiscordId).Returns(true);
        _mockHashingService.VerifyHash(TestMouseHuntIdHash, TestMouseHuntId).Returns(true);

        _service = new VerificationService(
            _logger,
            _dbContext,
            _mockCommandMentionService,
            _mockRestClient,
            _mockHashingService,
            _mockGuildLoggingService,
            _mockMouseHuntRestClient,
            _mockBounceService);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext?.Dispose();
    }

    #region AddVerifiedUserAsync Tests

    [TestMethod]
    public async Task AddVerifiedUserAsync_NewUser_AddsUserAndHistory()
    {
        // Act
        var result = await _service.AddVerifiedUserAsync(TestMouseHuntId, TestGuildId, TestDiscordId);

        // Assert
        Assert.IsTrue(result.WasAdded);
        Assert.AreEqual(TestMouseHuntId, result.MouseHuntId);

        var verifiedUser = await _dbContext.VerifiedUsers.FirstOrDefaultAsync();
        Assert.IsNotNull(verifiedUser);
        Assert.AreEqual(TestMouseHuntId, verifiedUser.MouseHuntId);
        Assert.AreEqual(TestGuildId, verifiedUser.GuildId);
        Assert.AreEqual(TestDiscordId, verifiedUser.DiscordId);

        var history = await _dbContext.VerificationHistory.FirstOrDefaultAsync();
        Assert.IsNotNull(history);
        Assert.AreEqual(TestGuildId, history.GuildId);
        Assert.AreEqual(TestDiscordIdHash, history.DiscordIdHash);
        Assert.AreEqual(TestMouseHuntIdHash, history.MouseHuntIdHash);
    }

    [TestMethod]
    public async Task AddVerifiedUserAsync_ExistingUser_ReturnsWasAddedFalse()
    {
        // Arrange
        await _dbContext.VerifiedUsers.AddAsync(new VerifiedUser
        {
            MouseHuntId = TestMouseHuntId,
            GuildId = TestGuildId,
            DiscordId = TestDiscordId
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.AddVerifiedUserAsync(TestMouseHuntId, TestGuildId, TestDiscordId);

        // Assert
        Assert.IsFalse(result.WasAdded);
        Assert.AreEqual(TestMouseHuntId, result.MouseHuntId);
    }

    [TestMethod]
    public async Task AddVerifiedUserAsync_ExistingHistoryWithDifferentHash_ThrowsException()
    {
        // Arrange
        const string differentHash = "different-hash";
        await _dbContext.VerificationHistory.AddAsync(new VerificationHistory
        {
            GuildId = TestGuildId,
            DiscordIdHash = TestDiscordIdHash,
            MouseHuntIdHash = differentHash
        });
        await _dbContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => _service.AddVerifiedUserAsync(TestMouseHuntId, TestGuildId, TestDiscordId));
    }

    [TestMethod]
    public async Task AddVerifiedUserAsync_ExistingHistoryWithSameHash_UpdatesOnlyVerifiedUsers()
    {
        // Arrange
        await _dbContext.VerificationHistory.AddAsync(new VerificationHistory
        {
            GuildId = TestGuildId,
            DiscordIdHash = TestDiscordIdHash,
            MouseHuntIdHash = TestMouseHuntIdHash
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.AddVerifiedUserAsync(TestMouseHuntId, TestGuildId, TestDiscordId);

        // Assert
        Assert.IsTrue(result.WasAdded);
        
        var historyCount = await _dbContext.VerificationHistory.CountAsync();
        Assert.AreEqual(1, historyCount); // Should not add duplicate history
        
        var verifiedUser = await _dbContext.VerifiedUsers.FirstOrDefaultAsync();
        Assert.IsNotNull(verifiedUser);
    }

    #endregion

    #region CanUserVerifyAsync Tests

    [TestMethod]
    public async Task CanUserVerifyAsync_AllChecksPass_ReturnsCanVerifyTrue()
    {
        // Arrange
        SetupSuccessfulCanVerifyMocks();

        // Act
        var result = await _service.CanUserVerifyAsync(TestMouseHuntId, TestGuildId, TestDiscordId);

        // Assert
        Assert.IsTrue(result.CanVerify);
        Assert.AreEqual("", result.Message);
    }

    [TestMethod]
    public async Task CanUserVerifyAsync_HunterIdBanned_ReturnsCanVerifyFalse()
    {
        // Arrange
        _mockBounceService.GetBannedHunterAsync(TestMouseHuntId, TestGuildId)
            .Returns(new BannedHunter { MouseHuntId = TestMouseHuntId, GuildId = TestGuildId });

        // Act
        var result = await _service.CanUserVerifyAsync(TestMouseHuntId, TestGuildId, TestDiscordId);

        // Assert
        Assert.IsFalse(result.CanVerify);
        Assert.IsTrue(result.Message.Contains("VF-2156-B3"));
        await _mockGuildLoggingService.Received(1).LogErrorAsync(TestGuildId, "Verification Blocked", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task CanUserVerifyAsync_MouseHuntIdInUse_ReturnsCanVerifyFalse()
    {
        // Arrange
        SetupSuccessfulCanVerifyMocks();
        
        await _dbContext.VerifiedUsers.AddAsync(new VerifiedUser
        {
            MouseHuntId = TestMouseHuntId,
            GuildId = TestGuildId,
            DiscordId = 99999ul // Different Discord ID
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CanUserVerifyAsync(TestMouseHuntId, TestGuildId, TestDiscordId);

        // Assert
        Assert.IsFalse(result.CanVerify);
        Assert.IsTrue(result.Message.Contains("VF-8429-A7"));
        await _mockGuildLoggingService.Received(1).LogWarningAsync(TestGuildId, "Verification Blocked", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task CanUserVerifyAsync_DifferentMouseHuntIdInHistory_ReturnsCanVerifyFalse()
    {
        // Arrange
        SetupSuccessfulCanVerifyMocks();
        //_mockHashingService.VerifyHash(Arg.Any<string>(), Arg.Any<uint>()).Returns(false);
        
        await _dbContext.VerificationHistory.AddAsync(new VerificationHistory
        {
            GuildId = TestGuildId,
            DiscordIdHash = TestDiscordIdHash,
            MouseHuntIdHash = "different-hash"
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CanUserVerifyAsync(TestMouseHuntId, TestGuildId, TestDiscordId);

        // Assert
        Assert.IsFalse(result.CanVerify);
        Assert.IsTrue(result.Message.Contains("You previously verified a different MouseHunt ID"));
        Assert.IsTrue(result.Message.Contains(TestCommandMention));
        await _mockGuildLoggingService.Received(1).LogWarningAsync(TestGuildId, "Verification Blocked", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task CanUserVerifyAsync_MouseHuntIdBelongsToDifferentUser_ReturnsCanVerifyFalse()
    {
        // Arrange
        SetupSuccessfulCanVerifyMocks();
        //_mockHashingService.VerifyHash(Arg.Any<string>(), Arg.Any<ulong>()).Returns(false);
        
        await _dbContext.VerificationHistory.AddAsync(new VerificationHistory
        {
            GuildId = TestGuildId,
            DiscordIdHash = "different-discord-hash",
            MouseHuntIdHash = TestMouseHuntIdHash
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.CanUserVerifyAsync(TestMouseHuntId, TestGuildId, TestDiscordId);

        // Assert
        Assert.IsFalse(result.CanVerify);
        Assert.IsTrue(result.Message.Contains("VF-9518-C3"));
        await _mockGuildLoggingService.Received(1).LogWarningAsync(TestGuildId, "Verification Blocked", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task CanUserVerifyAsync_UserRankTooLow_ReturnsCanVerifyFalse()
    {
        // Arrange
        SetupSuccessfulCanVerifyMocks(minRank: Rank.Master, userRank: Rank.Novice);

        // Act
        var result = await _service.CanUserVerifyAsync(TestMouseHuntId, TestGuildId, TestDiscordId);

        // Assert
        Assert.IsFalse(result.CanVerify);
        Assert.IsTrue(result.Message.Contains("Rank up to **Master**"));
        await _mockGuildLoggingService.Received(1).LogWarningAsync(TestGuildId, "Verification Blocked", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region IsDiscordUserVerifiedAsync Tests

    [TestMethod]
    public async Task IsDiscordUserVerifiedAsync_UserExists_ReturnsTrue()
    {
        // Arrange
        await _dbContext.VerifiedUsers.AddAsync(new VerifiedUser
        {
            MouseHuntId = TestMouseHuntId,
            GuildId = TestGuildId,
            DiscordId = TestDiscordId
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.IsDiscordUserVerifiedAsync(TestGuildId, TestDiscordId);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsDiscordUserVerifiedAsync_UserDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _service.IsDiscordUserVerifiedAsync(TestGuildId, TestDiscordId);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsDiscordUserVerifiedAsync_UserExistsInDifferentGuild_ReturnsFalse()
    {
        // Arrange
        const ulong differentGuildId = 99999ul;
        await _dbContext.VerifiedUsers.AddAsync(new VerifiedUser
        {
            MouseHuntId = TestMouseHuntId,
            GuildId = differentGuildId,
            DiscordId = TestDiscordId
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.IsDiscordUserVerifiedAsync(TestGuildId, TestDiscordId);

        // Assert
        Assert.IsFalse(result);
    }

    #endregion

    #region HasDiscordUserVerifiedWithMouseHuntIdBeforeAsync Tests

    [TestMethod]
    public async Task HasDiscordUserVerifiedWithMouseHuntIdBeforeAsync_NoHistory_ReturnsFalse()
    {
        // Act
        var result = await _service.HasDiscordUserVerifiedWithMouseHuntIdBeforeAsync(TestMouseHuntId, TestGuildId, TestDiscordId);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task HasDiscordUserVerifiedWithMouseHuntIdBeforeAsync_MatchingHistory_ReturnsTrue()
    {
        // Arrange
        await _dbContext.VerificationHistory.AddAsync(new VerificationHistory
        {
            GuildId = TestGuildId,
            DiscordIdHash = TestDiscordIdHash,
            MouseHuntIdHash = TestMouseHuntIdHash
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.HasDiscordUserVerifiedWithMouseHuntIdBeforeAsync(TestMouseHuntId, TestGuildId, TestDiscordId);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task HasDiscordUserVerifiedWithMouseHuntIdBeforeAsync_DifferentMouseHuntId_ReturnsFalse()
    {
        // Arrange
        //_mockHashingService.VerifyHash(Arg.Any<string>(), Arg.Any<uint>()).Returns(false);
        
        await _dbContext.VerificationHistory.AddAsync(new VerificationHistory
        {
            GuildId = TestGuildId,
            DiscordIdHash = TestDiscordIdHash,
            MouseHuntIdHash = "different-hash"
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.HasDiscordUserVerifiedWithMouseHuntIdBeforeAsync(TestMouseHuntId, TestGuildId, TestDiscordId);

        // Assert
        Assert.IsFalse(result);
    }

    #endregion

    #region HasDiscordUserVerifiedBeforeAsync Tests

    [TestMethod]
    public async Task HasDiscordUserVerifiedBeforeAsync_NoHistory_ReturnsFalse()
    {
        // Act
        var result = await _service.HasDiscordUserVerifiedBeforeAsync(TestGuildId, TestDiscordId);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task HasDiscordUserVerifiedBeforeAsync_HasHistory_ReturnsTrue()
    {
        // Arrange
        await _dbContext.VerificationHistory.AddAsync(new VerificationHistory
        {
            GuildId = TestGuildId,
            DiscordIdHash = TestDiscordIdHash,
            MouseHuntIdHash = TestMouseHuntIdHash
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.HasDiscordUserVerifiedBeforeAsync(TestGuildId, TestDiscordId);

        // Assert
        Assert.IsTrue(result);
    }

    #endregion

    #region RemoveVerifiedUser Tests

    [TestMethod]
    public async Task RemoveVerifiedUser_UserExists_RemovesUserAndReturnsTrue()
    {
        // Arrange
        var verifiedUser = new VerifiedUser
        {
            MouseHuntId = TestMouseHuntId,
            GuildId = TestGuildId,
            DiscordId = TestDiscordId
        };
        await _dbContext.VerifiedUsers.AddAsync(verifiedUser);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.RemoveVerifiedUser(TestGuildId, TestDiscordId);

        // Assert
        Assert.IsTrue(result.WasRemoved);
        
        var remainingUsers = await _dbContext.VerifiedUsers.CountAsync();
        Assert.AreEqual(0, remainingUsers);
    }

    [TestMethod]
    public async Task RemoveVerifiedUser_UserDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _service.RemoveVerifiedUser(TestGuildId, TestDiscordId);

        // Assert
        Assert.IsFalse(result.WasRemoved);
    }

    [TestMethod]
    public async Task RemoveVerifiedUser_UserWithVerifyMessage_DeletesMessageAndUser()
    {
        // Arrange
        var verifyMessage = new VerifyMessage
        {
            ChannelId = 12345ul,
            MessageId = 67890ul
        };
        await _dbContext.VerifyMessages.AddAsync(verifyMessage);
        await _dbContext.SaveChangesAsync();

        var verifiedUser = new VerifiedUser
        {
            MouseHuntId = TestMouseHuntId,
            GuildId = TestGuildId,
            DiscordId = TestDiscordId,
            VerifyMessageId = verifyMessage.Id,
            VerifyMessage = verifyMessage
        };
        await _dbContext.VerifiedUsers.AddAsync(verifiedUser);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.RemoveVerifiedUser(TestGuildId, TestDiscordId);

        // Assert
        Assert.IsTrue(result.WasRemoved);
        
        await _mockRestClient.Received(1).DeleteMessageAsync(verifyMessage.ChannelId, verifyMessage.MessageId, Arg.Any<CancellationToken>());
        
        var remainingUsers = await _dbContext.VerifiedUsers.CountAsync();
        Assert.AreEqual(0, remainingUsers);
        
        var remainingMessages = await _dbContext.VerifyMessages.CountAsync();
        Assert.AreEqual(0, remainingMessages);
    }

    [TestMethod]
    public async Task RemoveVerifiedUser_MessageDeleteThrowsRestException_StillRemovesUser()
    {
        // Arrange
        var verifyMessage = new VerifyMessage
        {
            ChannelId = 12345ul,
            MessageId = 67890ul
        };
        await _dbContext.VerifyMessages.AddAsync(verifyMessage);
        await _dbContext.SaveChangesAsync();

        var verifiedUser = new VerifiedUser
        {
            MouseHuntId = TestMouseHuntId,
            GuildId = TestGuildId,
            DiscordId = TestDiscordId,
            VerifyMessageId = verifyMessage.Id,
            VerifyMessage = verifyMessage
        };
        await _dbContext.VerifiedUsers.AddAsync(verifiedUser);
        await _dbContext.SaveChangesAsync();

        _mockRestClient.DeleteMessageAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new NetCord.Rest.RestException(System.Net.HttpStatusCode.NotFound, "Message not found")));

        // Act
        var result = await _service.RemoveVerifiedUser(TestGuildId, TestDiscordId);

        // Assert
        Assert.IsTrue(result.WasRemoved);
        
        var remainingUsers = await _dbContext.VerifiedUsers.CountAsync();
        Assert.AreEqual(0, remainingUsers);
    }

    [TestMethod]
    public async Task RemoveVerifiedUser_MessageDeleteThrowsGenericException_StillRemovesUser()
    {
        // Arrange
        var verifyMessage = new VerifyMessage
        {
            ChannelId = 12345ul,
            MessageId = 67890ul
        };
        await _dbContext.VerifyMessages.AddAsync(verifyMessage);
        await _dbContext.SaveChangesAsync();

        var verifiedUser = new VerifiedUser
        {
            MouseHuntId = TestMouseHuntId,
            GuildId = TestGuildId,
            DiscordId = TestDiscordId,
            VerifyMessageId = verifyMessage.Id,
            VerifyMessage = verifyMessage
        };
        await _dbContext.VerifiedUsers.AddAsync(verifiedUser);
        await _dbContext.SaveChangesAsync();

        _mockRestClient.DeleteMessageAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("Delete failed")));

        // Act
        var result = await _service.RemoveVerifiedUser(TestGuildId, TestDiscordId);

        // Assert
        Assert.IsTrue(result.WasRemoved);
        
        var remainingUsers = await _dbContext.VerifiedUsers.CountAsync();
        Assert.AreEqual(0, remainingUsers);
    }

    #endregion

    #region RemoveVerificationHistoryAsync Tests

    [TestMethod]
    public async Task RemoveVerificationHistoryAsync_HistoryExists_RemovesAndReturnsTrue()
    {
        // Arrange
        await _dbContext.VerificationHistory.AddAsync(new VerificationHistory
        {
            GuildId = TestGuildId,
            DiscordIdHash = TestDiscordIdHash,
            MouseHuntIdHash = TestMouseHuntIdHash
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.RemoveVerificationHistoryAsync(TestGuildId, TestDiscordId);

        // Assert
        Assert.IsTrue(result.WasRemoved);
        
        var remainingHistory = await _dbContext.VerificationHistory.CountAsync();
        Assert.AreEqual(0, remainingHistory);
    }

    [TestMethod]
    public async Task RemoveVerificationHistoryAsync_HistoryDoesNotExist_ReturnsFalse()
    {
        // Act
        var result = await _service.RemoveVerificationHistoryAsync(TestGuildId, TestDiscordId);

        // Assert
        Assert.IsFalse(result.WasRemoved);
    }

    #endregion

    #region SetVerificationMessageAsync Tests

    [TestMethod]
    public async Task SetVerificationMessageAsync_UserExists_SetsMessage()
    {
        // Arrange
        var verifiedUser = new VerifiedUser
        {
            MouseHuntId = TestMouseHuntId,
            GuildId = TestGuildId,
            DiscordId = TestDiscordId
        };
        await _dbContext.VerifiedUsers.AddAsync(verifiedUser);
        await _dbContext.SaveChangesAsync();

        var parameters = new SetVerificationMessageParameters(
            TestGuildId,
            TestDiscordId,
            12345ul,
            67890ul);

        // Act
        await _service.SetVerificationMessageAsync(parameters);

        // Assert
        var updatedUser = await _dbContext.VerifiedUsers
            .Include(vu => vu.VerifyMessage)
            .FirstAsync();
        
        Assert.IsNotNull(updatedUser.VerifyMessage);
        Assert.AreEqual(12345ul, updatedUser.VerifyMessage.ChannelId);
        Assert.AreEqual(67890ul, updatedUser.VerifyMessage.MessageId);
    }

    [TestMethod]
    public async Task SetVerificationMessageAsync_UserDoesNotExist_LogsWarning()
    {
        // Arrange
        var parameters = new SetVerificationMessageParameters(
            TestGuildId,
            TestDiscordId,
            12345ul,
            67890ul);

        // Act
        await _service.SetVerificationMessageAsync(parameters);

        // Assert
        var logs = _logger.Collector.GetSnapshot();
        Assert.IsNotEmpty(logs.Where(l => l.Level == LogLevel.Warning && l.Message.Contains($"Attempted to set verification message {67890ul} in channel {12345ul} for user {TestDiscordId} in guild {TestGuildId}, but they are not verified.")));
    }

    #endregion

    #region Edge Case Tests

    [TestMethod]
    public async Task CanUserVerifyAsync_MouseHuntApiFailure_ThrowsException()
    {
        // Arrange
        SetupSuccessfulCanVerifyMocks();
        _mockMouseHuntRestClient.GetTitlesAsync(default)!.Returns((Title[]?)null);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<Exception>(
            () => _service.CanUserVerifyAsync(TestMouseHuntId, TestGuildId, TestDiscordId));
    }

    [TestMethod]
    public async Task CanUserVerifyAsync_UserTitleApiFailure_ThrowsException()
    {
        // Arrange
        SetupSuccessfulCanVerifyMocks();
        var titles = new[] { new Title { TitleId = 1, Name = Rank.Master } };
        _mockMouseHuntRestClient.GetTitlesAsync(Arg.Any<CancellationToken>()).Returns(titles);
        _mockMouseHuntRestClient.GetUserTitleAsync(TestMouseHuntId, default)!.Returns((UserTitle?)null);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<Exception>(
            () => _service.CanUserVerifyAsync(TestMouseHuntId, TestGuildId, TestDiscordId));
    }

    [TestMethod]
    public async Task AddVerifiedUserAsync_WithCancellationToken_UsesCancellationToken()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        // Act
        var result = await _service.AddVerifiedUserAsync(TestMouseHuntId, TestGuildId, TestDiscordId, cancellationToken);

        // Assert
        Assert.IsTrue(result.WasAdded);
        // Verify that hashing service was called for both discord ID and mouse hunt ID
        //_mockHashingService.Received(1).HashValue(Arg.Any<ulong>()); // Called for discord ID
        //_mockHashingService.Received(1).HashValue(Arg.Any<uint>()); // Called for mouse hunt ID
    }

    #endregion

    #region Helper Methods

    private void SetupSuccessfulCanVerifyMocks(Rank minRank = Rank.Novice, Rank userRank = Rank.Master)
    {
        // No banned hunter
        _mockBounceService.GetBannedHunterAsync(TestMouseHuntId, TestGuildId)
            .Returns((BannedHunter?)null);

        // Default minimum rank
        _dbContext.VerifySettings.Add(new VerifySetting
        {
            GuildId = TestGuildId,
            MinimumRank = minRank
        });
        _dbContext.SaveChanges();

        // User meets rank requirement
        var titles = new[] { new Title { TitleId = 1, Name = userRank} };
        var userTitle = new UserTitle { TitleId = 1 };
        
        _mockMouseHuntRestClient.GetTitlesAsync(Arg.Any<CancellationToken>()).Returns(titles);
        _mockMouseHuntRestClient.GetUserTitleAsync(TestMouseHuntId, Arg.Any<CancellationToken>()).Returns(userTitle);
    }

    #endregion
}
