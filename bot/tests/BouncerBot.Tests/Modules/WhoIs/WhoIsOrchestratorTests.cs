using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using BouncerBot.Modules.WhoIs;
using BouncerBot.Db.Models;
using NSubstitute.ExceptionExtensions;

namespace BouncerBot.Tests.Modules.WhoIs;

[TestClass]
public class WhoIsOrchestratorTests
{
    private IWhoIsService _mockWhoIsService = null!;
    private WhoIsOrchestrator _orchestrator = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockWhoIsService = Substitute.For<IWhoIsService>();
        _orchestrator = new WhoIsOrchestrator(_mockWhoIsService);
    }

    [TestMethod]
    public async Task GetHunterIdByDiscordUserAsync_WhenUserIsVerified_ReturnsSuccessWithHunterId()
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

        _mockWhoIsService.GetVerifiedUserByDiscordIdAsync(guildId, discordId, Arg.Any<CancellationToken>())
            .Returns(verifiedUser);

        // Act
        var result = await _orchestrator.GetHunterIdAsync(guildId, discordId);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains(discordId.ToString()));
        Assert.IsTrue(result.Message.Contains(mouseHuntId.ToString()));
        Assert.IsTrue(result.Message.Contains("p.mshnt.ca"));
    }

    [TestMethod]
    public async Task GetHunterIdByDiscordUserAsync_WhenUserIsNotVerified_ReturnsFailureMessage()
    {
        // Arrange
        const ulong guildId = 123456789;
        const ulong discordId = 987654321;

        _mockWhoIsService.GetVerifiedUserByDiscordIdAsync(guildId, discordId, Arg.Any<CancellationToken>())
            .Returns((VerifiedUser?)null);

        // Act
        var result = await _orchestrator.GetHunterIdAsync(guildId, discordId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("not verified"));
        Assert.IsTrue(result.Message.Contains(discordId.ToString()));
    }

    [TestMethod]
    public async Task GetHunterIdByDiscordUserAsync_WhenServiceThrowsException_ReturnsErrorMessage()
    {
        // Arrange
        const ulong guildId = 123456789;
        const ulong discordId = 987654321;
        const string errorMessage = "Database connection failed";

        _mockWhoIsService.GetVerifiedUserByDiscordIdAsync(guildId, discordId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await _orchestrator.GetHunterIdAsync(guildId, discordId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("error occurred"));
        Assert.IsTrue(result.Message.Contains(errorMessage));
    }

    [TestMethod]
    public async Task GetDiscordUserByHunterIdAsync_WhenHunterIsVerified_ReturnsSuccessWithDiscordUser()
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

        _mockWhoIsService.GetVerifiedUserByMouseHuntIdAsync(guildId, mouseHuntId, Arg.Any<CancellationToken>())
            .Returns(verifiedUser);

        // Act
        var result = await _orchestrator.GetUserIdAsync(guildId, mouseHuntId);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains(discordId.ToString()));
        Assert.IsTrue(result.Message.Contains(mouseHuntId.ToString()));
        Assert.IsTrue(result.Message.Contains("p.mshnt.ca"));
    }

    [TestMethod]
    public async Task GetDiscordUserByHunterIdAsync_WhenHunterIsNotVerified_ReturnsFailureMessage()
    {
        // Arrange
        const ulong guildId = 123456789;
        const uint mouseHuntId = 12345;

        _mockWhoIsService.GetVerifiedUserByMouseHuntIdAsync(guildId, mouseHuntId, Arg.Any<CancellationToken>())
            .Returns((VerifiedUser?)null);

        // Act
        var result = await _orchestrator.GetUserIdAsync(guildId, mouseHuntId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("not verified"));
        Assert.IsTrue(result.Message.Contains(mouseHuntId.ToString()));
        Assert.IsTrue(result.Message.Contains("p.mshnt.ca"));
    }

    [TestMethod]
    public async Task GetDiscordUserByHunterIdAsync_WhenServiceThrowsException_ReturnsErrorMessage()
    {
        // Arrange
        const ulong guildId = 123456789;
        const uint mouseHuntId = 12345;
        const string errorMessage = "Database timeout";

        _mockWhoIsService.GetVerifiedUserByMouseHuntIdAsync(guildId, mouseHuntId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await _orchestrator.GetUserIdAsync(guildId, mouseHuntId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("error occurred"));
        Assert.IsTrue(result.Message.Contains(errorMessage));
    }

    [TestMethod]
    public async Task GetHunterIdByDiscordUserAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        const ulong guildId = 123456789;
        const ulong discordId = 987654321;
        var cancellationToken = new CancellationToken();

        _mockWhoIsService.GetVerifiedUserByDiscordIdAsync(guildId, discordId, cancellationToken)
            .Returns((VerifiedUser?)null);

        // Act
        await _orchestrator.GetHunterIdAsync(guildId, discordId, cancellationToken);

        // Assert
        await _mockWhoIsService.Received(1).GetVerifiedUserByDiscordIdAsync(guildId, discordId, cancellationToken);
    }

    [TestMethod]
    public async Task GetDiscordUserByHunterIdAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        const ulong guildId = 123456789;
        const uint mouseHuntId = 12345;
        var cancellationToken = new CancellationToken();

        _mockWhoIsService.GetVerifiedUserByMouseHuntIdAsync(guildId, mouseHuntId, cancellationToken)
            .Returns((VerifiedUser?)null);

        // Act
        await _orchestrator.GetUserIdAsync(guildId, mouseHuntId, cancellationToken);

        // Assert
        await _mockWhoIsService.Received(1).GetVerifiedUserByMouseHuntIdAsync(guildId, mouseHuntId, cancellationToken);
    }
}
