using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using BouncerBot.Modules.Verify;
using BouncerBot.Rest;
using BouncerBot.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NetCord.Rest;
using BouncerBot.Rest.Models;

[TestClass]
public class VerificationOrchestratorTests
{
    private IVerificationService _verificationService = null!;
    private IMouseHuntRestClient _mouseHuntRestClient = null!;
    private IGuildLoggingService _guildLoggingService = null!;
    private VerificationOrchestrator _orchestrator = null!;

    [TestInitialize]
    public void Setup()
    {
        _verificationService = Substitute.For<IVerificationService>();
        _mouseHuntRestClient = Substitute.For<IMouseHuntRestClient>();
        _guildLoggingService = Substitute.For<IGuildLoggingService>();
        _orchestrator = new VerificationOrchestrator(_verificationService, _mouseHuntRestClient, _guildLoggingService);
    }

    [TestMethod]
    public async Task ProcessVerificationAsync_Self_SuccessfulVerification_ReturnsSuccess()
    {
        // Arrange
        var parameters = new VerificationParameters
        {
            MouseHuntId = 123,
            DiscordUserId = 456,
            GuildId = 789,
            Phrase = "test phrase"
        };

        var snuid = new UserSnuIdInfo { SnUserId = "abc" };
        var corkboard = new Corkboard
        {
            CorkboardMessages = [
                new CorkboardMessage { SnUserId = "abc", Body = "test phrase" }
            ]
        };

        _mouseHuntRestClient.GetUserSnuIdAsync(123, Arg.Any<CancellationToken>()).Returns(snuid);
        _mouseHuntRestClient.GetCorkboardAsync(123, Arg.Any<CancellationToken>()).Returns(corkboard);
        _verificationService.AddVerifiedUserAsync(123, 789, 456, Arg.Any<CancellationToken>())
            .Returns(new VerificationAddResult(true, 123));
        _guildLoggingService.LogAsync(789, LogType.Verification, Arg.Any<MessageProperties>(), Arg.Any<CancellationToken>())
            .Returns(new RestMessage(new NetCord.JsonModels.JsonMessage(), null!));

        // Act
        var result = await _orchestrator.ProcessVerificationAsync(VerificationType.Self, parameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("Success"));
        await _verificationService.Received(1).AddVerifiedUserAsync(123, 789, 456, Arg.Any<CancellationToken>());
        await _verificationService.Received(1).SetVerificationMessageAsync(Arg.Is<SetVerificationMessageParameters>(p =>
            p.GuildId == 789 && p.DiscordUserId == 456 && p.ChannelId == 2 && p.MessageId == 1));
    }

    [TestMethod]
    public async Task ProcessVerificationAsync_Self_PhraseMatchButSnuidMismatch_LogsAndReturnsFailure()
    {
        // Arrange
        var parameters = new VerificationParameters
        {
            MouseHuntId = 123,
            DiscordUserId = 456,
            GuildId = 789,
            Phrase = "test phrase"
        };

        var snuid = new UserSnuIdInfo { SnUserId = "abc" };
        var corkboard = new Corkboard
        {
            CorkboardMessages = new[]
            {
                new CorkboardMessage { SnUserId = "def", Body = "test phrase" }
            }
        };

        _mouseHuntRestClient.GetUserSnuIdAsync(123, Arg.Any<CancellationToken>()).Returns(snuid);
        _mouseHuntRestClient.GetCorkboardAsync(123, Arg.Any<CancellationToken>()).Returns(corkboard);

        // Act
        var result = await _orchestrator.ProcessVerificationAsync(VerificationType.Self, parameters);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Linking failed"));
        await _guildLoggingService.Received(1).LogAsync(789, LogType.General, Arg.Any<MessageProperties>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task ProcessVerificationAsync_Self_PhraseMismatch_ReturnsFailure()
    {
        // Arrange
        var parameters = new VerificationParameters
        {
            MouseHuntId = 123,
            DiscordUserId = 456,
            GuildId = 789,
            Phrase = "test phrase"
        };

        var snuid = new UserSnuIdInfo { SnUserId = "abc" };
        var corkboard = new Corkboard
        {
            CorkboardMessages = new[]
            {
                new CorkboardMessage { SnUserId = "abc", Body = "other phrase" }
            }
        };

        _mouseHuntRestClient.GetUserSnuIdAsync(123, Arg.Any<CancellationToken>()).Returns(snuid);
        _mouseHuntRestClient.GetCorkboardAsync(123, Arg.Any<CancellationToken>()).Returns(corkboard);

        // Act
        var result = await _orchestrator.ProcessVerificationAsync(VerificationType.Self, parameters);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("Linking failed"));
        await _guildLoggingService.DidNotReceive().LogAsync(Arg.Any<ulong>(), LogType.General, Arg.Any<MessageProperties>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task ProcessVerificationAsync_Add_UserNotVerified_AddsAndReturnsSuccess()
    {
        // Arrange
        var parameters = new VerificationParameters
        {
            MouseHuntId = 123,
            DiscordUserId = 456,
            GuildId = 789
        };

        _verificationService.IsDiscordUserVerifiedAsync(789, 456, Arg.Any<CancellationToken>()).Returns(false);
        _verificationService.AddVerifiedUserAsync(123, 789, 456, Arg.Any<CancellationToken>())
            .Returns(new VerificationAddResult(true, 123));
        _guildLoggingService.LogAsync(789, LogType.Verification, Arg.Any<MessageProperties>(), Arg.Any<CancellationToken>())
            .Returns(new RestMessage { Id = 1, ChannelId = 2 });

        // Act
        var result = await _orchestrator.ProcessVerificationAsync(VerificationType.Add, parameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("Verified"));
        await _verificationService.Received(1).AddVerifiedUserAsync(123, 789, 456, Arg.Any<CancellationToken>());
        await _verificationService.Received(1).SetVerificationMessageAsync(Arg.Any<SetVerificationMessageParameters>());
    }

    [TestMethod]
    public async Task ProcessVerificationAsync_Add_UserAlreadyVerified_ReturnsFailure()
    {
        // Arrange
        var parameters = new VerificationParameters
        {
            MouseHuntId = 123,
            DiscordUserId = 456,
            GuildId = 789
        };

        _verificationService.IsDiscordUserVerifiedAsync(789, 456, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _orchestrator.ProcessVerificationAsync(VerificationType.Add, parameters);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("already verified"));
        await _verificationService.DidNotReceive().AddVerifiedUserAsync(Arg.Any<uint>(), Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task ProcessVerificationAsync_Remove_UserVerified_RemovesAndReturnsSuccess()
    {
        // Arrange
        var parameters = new VerificationParameters
        {
            MouseHuntId = 123,
            DiscordUserId = 456,
            GuildId = 789
        };

        _verificationService.IsDiscordUserVerifiedAsync(789, 456, Arg.Any<CancellationToken>()).Returns(true);
        _verificationService.RemoveVerifiedUser(789, 456, Arg.Any<CancellationToken>())
            .Returns(new VerificationRemoveResult(true, 123));

        // Act
        var result = await _orchestrator.ProcessVerificationAsync(VerificationType.Remove, parameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Message.Contains("Removed verification"));
        await _verificationService.Received(1).RemoveVerifiedUser(789, 456, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task ProcessVerificationAsync_Remove_UserNotVerified_ReturnsFailure()
    {
        // Arrange
        var parameters = new VerificationParameters
        {
            MouseHuntId = 123,
            DiscordUserId = 456,
            GuildId = 789
        };

        _verificationService.IsDiscordUserVerifiedAsync(789, 456, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _orchestrator.ProcessVerificationAsync(VerificationType.Remove, parameters);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Message.Contains("not verified"));
        await _verificationService.DidNotReceive().RemoveVerifiedUser(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<CancellationToken>());
    }
}
