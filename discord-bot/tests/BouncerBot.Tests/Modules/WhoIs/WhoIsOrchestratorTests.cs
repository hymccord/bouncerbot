using BouncerBot.Db.Models;
using BouncerBot.Modules.WhoIs;
using NSubstitute;
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
        const ulong GuildId = 123456789;
        const ulong DiscordId = 987654321;
        const uint MouseHuntId = 12345;

        var verifiedUser = new VerifiedUser
        {
            GuildId = GuildId,
            DiscordId = DiscordId,
            MouseHuntId = MouseHuntId
        };

        _mockWhoIsService.GetVerifiedUserByDiscordIdAsync(GuildId, DiscordId, Arg.Any<CancellationToken>())
            .Returns(verifiedUser);

        // Act
        var result = await _orchestrator.GetHunterIdAsync(GuildId, DiscordId);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains(DiscordId.ToString()));
        Assert.IsTrue(result.Message.Contains(MouseHuntId.ToString()));
        Assert.IsTrue(result.Message.Contains("p.mshnt.ca"));
    }

    [TestMethod]
    public async Task GetHunterIdByDiscordUserAsync_WhenUserIsNotVerified_ReturnsFailureMessage()
    {
        // Arrange
        const ulong GuildId = 123456789;
        const ulong DiscordId = 987654321;

        _mockWhoIsService.GetVerifiedUserByDiscordIdAsync(GuildId, DiscordId, Arg.Any<CancellationToken>())
            .Returns((VerifiedUser?)null);

        // Act
        var result = await _orchestrator.GetHunterIdAsync(GuildId, DiscordId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("not verified"));
        Assert.IsTrue(result.Message.Contains(DiscordId.ToString()));
    }

    [TestMethod]
    public async Task GetHunterIdByDiscordUserAsync_WhenServiceThrowsException_ReturnsErrorMessage()
    {
        // Arrange
        const ulong GuildId = 123456789;
        const ulong DiscordId = 987654321;
        const string ErrorMessage = "Database connection failed";

        _mockWhoIsService.GetVerifiedUserByDiscordIdAsync(GuildId, DiscordId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(ErrorMessage));

        // Act
        var result = await _orchestrator.GetHunterIdAsync(GuildId, DiscordId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("error occurred"));
        Assert.IsTrue(result.Message.Contains(ErrorMessage));
    }

    [TestMethod]
    public async Task GetDiscordUserByHunterIdAsync_WhenHunterIsVerified_ReturnsSuccessWithDiscordUser()
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

        _mockWhoIsService.GetVerifiedUserByMouseHuntIdAsync(GuildId, MouseHuntId, Arg.Any<CancellationToken>())
            .Returns(verifiedUser);

        // Act
        var result = await _orchestrator.GetUserIdAsync(GuildId, MouseHuntId);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains(DiscordId.ToString()));
        Assert.IsTrue(result.Message.Contains(MouseHuntId.ToString()));
        Assert.IsTrue(result.Message.Contains("p.mshnt.ca"));
    }

    [TestMethod]
    public async Task GetDiscordUserByHunterIdAsync_WhenHunterIsNotVerified_ReturnsFailureMessage()
    {
        // Arrange
        const ulong GuildId = 123456789;
        const uint MouseHuntId = 12345;

        _mockWhoIsService.GetVerifiedUserByMouseHuntIdAsync(GuildId, MouseHuntId, Arg.Any<CancellationToken>())
            .Returns((VerifiedUser?)null);

        // Act
        var result = await _orchestrator.GetUserIdAsync(GuildId, MouseHuntId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("not verified"));
        Assert.IsTrue(result.Message.Contains(MouseHuntId.ToString()));
        Assert.IsTrue(result.Message.Contains("p.mshnt.ca"));
    }

    [TestMethod]
    public async Task GetDiscordUserByHunterIdAsync_WhenServiceThrowsException_ReturnsErrorMessage()
    {
        // Arrange
        const ulong GuildId = 123456789;
        const uint MouseHuntId = 12345;
        const string ErrorMessage = "Database timeout";

        _mockWhoIsService.GetVerifiedUserByMouseHuntIdAsync(GuildId, MouseHuntId, Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(ErrorMessage));

        // Act
        var result = await _orchestrator.GetUserIdAsync(GuildId, MouseHuntId);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("error occurred"));
        Assert.IsTrue(result.Message.Contains(ErrorMessage));
    }

    [TestMethod]
    public async Task GetHunterIdByDiscordUserAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        const ulong GuildId = 123456789;
        const ulong DiscordId = 987654321;
        var cancellationToken = new CancellationToken();

        _mockWhoIsService.GetVerifiedUserByDiscordIdAsync(GuildId, DiscordId, cancellationToken)
            .Returns((VerifiedUser?)null);

        // Act
        await _orchestrator.GetHunterIdAsync(GuildId, DiscordId, cancellationToken);

        // Assert
        await _mockWhoIsService.Received(1).GetVerifiedUserByDiscordIdAsync(GuildId, DiscordId, cancellationToken);
    }

    [TestMethod]
    public async Task GetDiscordUserByHunterIdAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        const ulong GuildId = 123456789;
        const uint MouseHuntId = 12345;
        var cancellationToken = new CancellationToken();

        _mockWhoIsService.GetVerifiedUserByMouseHuntIdAsync(GuildId, MouseHuntId, cancellationToken)
            .Returns((VerifiedUser?)null);

        // Act
        await _orchestrator.GetUserIdAsync(GuildId, MouseHuntId, cancellationToken);

        // Assert
        await _mockWhoIsService.Received(1).GetVerifiedUserByMouseHuntIdAsync(GuildId, MouseHuntId, cancellationToken);
    }
}
