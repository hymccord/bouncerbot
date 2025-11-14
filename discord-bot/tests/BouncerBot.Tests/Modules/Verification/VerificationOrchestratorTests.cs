using BouncerBot.Modules.Verification;
using BouncerBot.Rest;
using BouncerBot.Rest.Models;
using BouncerBot.Services;
using NetCord.Rest;
using NSubstitute;

namespace BouncerBot.Tests.Modules.Verification;

[TestClass]
public class VerificationOrchestratorTests
{
    private readonly IGuildLoggingService _guildLoggingService;
    private readonly IMouseHuntRestClient _mouseHuntRestClient;
    private readonly IRoleService _roleService;
    private readonly IVerificationService _verificationService;
    private readonly VerificationOrchestrator _sut;

    public VerificationOrchestratorTests()
    {    
        _guildLoggingService = Substitute.For<IGuildLoggingService>();
        _mouseHuntRestClient = Substitute.For<IMouseHuntRestClient>();
        _roleService = Substitute.For<IRoleService>();
        _verificationService = Substitute.For<IVerificationService>();

        _sut = new VerificationOrchestrator(_guildLoggingService, _mouseHuntRestClient, _roleService, _verificationService);
    }

    [TestMethod]
    public async Task ProcessVerificationAsync_Self_SuccessfulVerification_ReturnsSuccess()
    {
        var mhid = 123u;
        var snuid = "abc";

        // Arrange
        var parameters = new VerificationParameters
        {
            MouseHuntId = mhid,
            DiscordUserId = 456,
            GuildId = 789,
            Phrase = "test phrase"
        };

        SetupGetUserSnuIdAsync(123, snuid);
        SetupGetCorkboardAsync(123, snuid, "test phrase");

        _verificationService.AddVerifiedUserAsync(mhid, 789, 456)
            .Returns(new VerificationAddResult(true, 123));
        _guildLoggingService.LogAsync(789, LogType.Verification, Arg.Any<MessageProperties>())
            .Returns(new LogMessageResult { ChannelId = 1001u, MessageId = 3u });

        // Act
        var result = await _sut.ProcessVerificationAsync(VerificationType.Self, parameters);

        // Assert
        Assert.IsTrue(result.Success);
        await _verificationService.Received(1).AddVerifiedUserAsync(mhid, 789, 456);
        await _roleService.Received(1).AddRoleAsync(456, 789, Role.Verified, default);
        await _verificationService.Received(1).SetVerificationMessageAsync(Arg.Is<SetVerificationMessageParameters>(p =>
            p.GuildId == 789 && p.DiscordUserId == 456 && p.ChannelId == 1001u && p.MessageId == 3u));
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

        SetupGetUserSnuIdAsync(123, "abc");
        SetupGetCorkboardAsync(123, "def", "test phrase");

        // Act
        var result = await _sut.ProcessVerificationAsync(VerificationType.Self, parameters);

        // Assert
        Assert.IsFalse(result.Success);
        await _guildLoggingService
            .Received(1)
            .LogWarningAsync(789, "Verification Blocked", Arg.Any<string>());
        await _roleService.DidNotReceiveWithAnyArgs().AddRoleAsync(default, default, default, default);
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

        SetupGetUserSnuIdAsync(123, "abc");
        SetupGetCorkboardAsync(123, "abc", "other phrase");

        // Act
        var result = await _sut.ProcessVerificationAsync(VerificationType.Self, parameters);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.Contains("Linking failed", result.Message);
        await _guildLoggingService.DidNotReceiveWithAnyArgs().LogAsync(default, default, default!, default);
        await _roleService.DidNotReceiveWithAnyArgs().AddRoleAsync(default, default, default, default);
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

        _verificationService.IsDiscordUserVerifiedAsync(789, 456).Returns(false);
        _verificationService.AddVerifiedUserAsync(123, 789, 456)
            .Returns(new VerificationAddResult(true, 123));
        _guildLoggingService.LogAsync(789, LogType.Verification, Arg.Any<MessageProperties>())
            .Returns(new LogMessageResult { ChannelId = 1u, MessageId = 2u });

        // Act
        var result = await _sut.ProcessVerificationAsync(VerificationType.Add, parameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.Contains("Verified", result.Message);
        await _verificationService.Received(1).AddVerifiedUserAsync(123, 789, 456, Arg.Any<CancellationToken>());
        await _roleService.Received(1).AddRoleAsync(456, 789, Role.Verified, default);
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
        var result = await _sut.ProcessVerificationAsync(VerificationType.Add, parameters);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.Contains("already verified", result.Message);
        await _verificationService.DidNotReceive().AddVerifiedUserAsync(Arg.Any<uint>(), Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<CancellationToken>());
        await _roleService.DidNotReceiveWithAnyArgs().AddRoleAsync(default, default, default, default);
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
            .Returns(new VerificationRemoveResult(true));

        // Act
        var result = await _sut.ProcessVerificationAsync(VerificationType.Remove, parameters);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.Contains("Removed verification", result.Message);
        await _verificationService.Received(1).RemoveVerifiedUser(789, 456, Arg.Any<CancellationToken>());
        await _roleService.Received(1).RemoveRoleAsync(456, 789, Role.Verified, default);
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
        var result = await _sut.ProcessVerificationAsync(VerificationType.Remove, parameters);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.Contains("not verified", result.Message);
        await _verificationService.DidNotReceive().RemoveVerifiedUser(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<CancellationToken>());
    }

    private void SetupGetUserSnuIdAsync(uint mouseHuntId, string snuid)
    {
        _mouseHuntRestClient.GetUserSnuIdAsync(mouseHuntId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new UserSnuIdInfo()
            {
                SnUserId = snuid
            }));
    }

    private void SetupGetCorkboardAsync(uint mouseHuntId, string snuid, string message)
    {
        _mouseHuntRestClient.GetCorkboardAsync(mouseHuntId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new Corkboard
            {
                CorkboardMessages = [
                    new() { SnUserId = snuid, Body = message }
                ]
            }));
    }
}
