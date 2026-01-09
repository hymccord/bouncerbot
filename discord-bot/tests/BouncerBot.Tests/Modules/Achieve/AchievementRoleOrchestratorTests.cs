using BouncerBot.Modules.Achieve;
using BouncerBot.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetCord;
using NetCord.Gateway;
using NSubstitute;
using System.Collections.Generic;

namespace BouncerBot.Tests.Modules.Achieve;

[TestClass]
public class AchievementRoleOrchestratorTests
{
    private IAchievementService _achievementService = null!;
    private IRoleService _roleService = null!;
    private IAchievementMessageService _achievementMessageService = null!;
    private IDiscordGatewayClient _gatewayClient = null!;
    private AchievementRoleOrchestrator _orchestrator = null!;

    private const uint TestMhid = 12345u;
    private const ulong TestUserId = 98765uL;
    private const ulong TestGuildId = 54321uL;
    private const AchievementRole TestAchievement = AchievementRole.Star;
    private const ulong TestRoleId = 11111uL;

    [TestInitialize]
    public void Setup()
    {
        _achievementService = Substitute.For<IAchievementService>();
        _roleService = Substitute.For<IRoleService>();
        _achievementMessageService = Substitute.For<IAchievementMessageService>();
        _gatewayClient = Substitute.For<IDiscordGatewayClient>();

        _orchestrator = new AchievementRoleOrchestrator(
            _achievementService,
            _roleService,
            _achievementMessageService,
            _gatewayClient);
    }

    [TestMethod]
    public async Task ProcessAchievementAsync_WhenUserHasAchievementAndNoRole_ReturnsSuccessAndAddsRole()
    {
        // Arrange
        _achievementService.HasAchievementAsync(TestMhid, TestAchievement, Arg.Any<CancellationToken>())
            .Returns(new StarProgress { IsComplete = true, CompletedLocations = 10, TotalLocations = 10, IncompleteLocations = [] });
        _roleService.HasRoleAsync(TestUserId, TestGuildId, Arg.Any<Role>())
            .Returns(false);

        // Act
        var result = await _orchestrator.ProcessAchievementAsync(TestMhid, TestUserId, TestGuildId, TestAchievement);

        // Assert
        Assert.AreEqual(ClaimResult.Success, result.Result);
        Assert.IsNotNull(result.Progress);
        Assert.IsTrue(result.Progress.IsComplete);
        await _roleService.Received(1).AddRoleAsync(TestUserId, TestGuildId, Arg.Any<Role>(), Arg.Any<CancellationToken>());
        await _achievementMessageService.Received(1).SendAchievementMessageAsync(TestUserId, TestGuildId, TestAchievement, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task ProcessAchievementAsync_WhenUserHasAchievementAndRole_ReturnsAlreadyHasRole()
    {
        // Arrange
        _achievementService.HasAchievementAsync(TestMhid, TestAchievement, Arg.Any<CancellationToken>())
            .Returns(new StarProgress { IsComplete = true, CompletedLocations = 10, TotalLocations = 10, IncompleteLocations = [] });
        _roleService.HasRoleAsync(TestUserId, TestGuildId, Arg.Any<Role>())
            .Returns(true);

        // Act
        var result = await _orchestrator.ProcessAchievementAsync(TestMhid, TestUserId, TestGuildId, TestAchievement);

        // Assert
        Assert.AreEqual(ClaimResult.AlreadyHasRole, result.Result);
        Assert.IsNotNull(result.Progress);
        await _roleService.DidNotReceive().AddRoleAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<Role>(), Arg.Any<CancellationToken>());
        await _achievementMessageService.DidNotReceive().SendAchievementMessageAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<AchievementRole>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task ProcessAchievementAsync_WhenUserDoesNotHaveAchievement_ReturnsNotAchieved()
    {
        // Arrange
        _achievementService.HasAchievementAsync(TestMhid, TestAchievement, Arg.Any<CancellationToken>())
            .Returns(new StarProgress { IsComplete = false, CompletedLocations = 5, TotalLocations = 10, IncompleteLocations = ["Location1", "Location2", "Location3", "Location4", "Location5"] });

        // Act
        var result = await _orchestrator.ProcessAchievementAsync(TestMhid, TestUserId, TestGuildId, TestAchievement);

        // Assert
        Assert.AreEqual(ClaimResult.NotAchieved, result.Result);
        Assert.IsNotNull(result.Progress);
        Assert.IsFalse(result.Progress.IsComplete);
        await _roleService.DidNotReceive().HasRoleAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<Role>());
        await _roleService.DidNotReceive().AddRoleAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<Role>(), Arg.Any<CancellationToken>());
        await _achievementMessageService.DidNotReceive().SendAchievementMessageAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<AchievementRole>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task GrantAchievementAsync_WhenUserDoesNotHaveRole_WithSendMessage_ReturnsSuccessAndAddsRoleWithMessage()
    {
        // Arrange
        _roleService.HasRoleAsync(TestUserId, TestGuildId, Arg.Any<Role>())
            .Returns(false);

        // Act
        var result = await _orchestrator.GrantAchievementAsync(TestUserId, TestGuildId, TestAchievement, NotificationMode.SendMessage);

        // Assert
        Assert.AreEqual(ClaimResult.Success, result);
        await _roleService.Received(1).AddRoleAsync(TestUserId, TestGuildId, Arg.Any<Role>(), Arg.Any<CancellationToken>());
        await _achievementMessageService.Received(1).SendAchievementMessageAsync(TestUserId, TestGuildId, TestAchievement, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task GrantAchievementAsync_WhenUserDoesNotHaveRole_WithSilent_ReturnsSuccessAndAddsRoleWithoutMessage()
    {
        // Arrange
        _roleService.HasRoleAsync(TestUserId, TestGuildId, Arg.Any<Role>())
            .Returns(false);

        // Act
        var result = await _orchestrator.GrantAchievementAsync(TestUserId, TestGuildId, TestAchievement, NotificationMode.Silent);

        // Assert
        Assert.AreEqual(ClaimResult.Success, result);
        await _roleService.Received(1).AddRoleAsync(TestUserId, TestGuildId, Arg.Any<Role>(), Arg.Any<CancellationToken>());
        await _achievementMessageService.DidNotReceive().SendAchievementMessageAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<AchievementRole>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task GrantAchievementAsync_WhenUserAlreadyHasRole_ReturnsAlreadyHasRole()
    {
        // Arrange
        _roleService.HasRoleAsync(TestUserId, TestGuildId, Arg.Any<Role>())
            .Returns(true);

        // Act
        var result = await _orchestrator.GrantAchievementAsync(TestUserId, TestGuildId, TestAchievement, NotificationMode.SendMessage);

        // Assert
        Assert.AreEqual(ClaimResult.AlreadyHasRole, result);
        await _roleService.DidNotReceive().AddRoleAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<Role>(), Arg.Any<CancellationToken>());
        await _achievementMessageService.DidNotReceive().SendAchievementMessageAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<AchievementRole>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task ProcessAchievementSilentlyAsync_WhenUserHasAchievementAndNoRole_ReturnsSuccessAndAddsRoleWithoutMessage()
    {
        // Arrange
        _achievementService.HasAchievementAsync(TestMhid, TestAchievement, Arg.Any<CancellationToken>())
            .Returns(new StarProgress { IsComplete = true, CompletedLocations = 10, TotalLocations = 10, IncompleteLocations = [] });
        _roleService.HasRoleAsync(TestUserId, TestGuildId, Arg.Any<Role>())
            .Returns(false);

        // Act
        var result = await _orchestrator.ProcessAchievementSilentlyAsync(TestMhid, TestUserId, TestGuildId, TestAchievement);

        // Assert
        Assert.AreEqual(ClaimResult.Success, result.Result);
        Assert.IsNotNull(result.Progress);
        await _roleService.Received(1).AddRoleAsync(TestUserId, TestGuildId, Arg.Any<Role>(), Arg.Any<CancellationToken>());
        await _achievementMessageService.DidNotReceive().SendAchievementMessageAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<AchievementRole>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task ProcessAchievementSilentlyAsync_WhenUserHasAchievementAndRole_ReturnsAlreadyHasRole()
    {
        // Arrange
        _achievementService.HasAchievementAsync(TestMhid, TestAchievement, Arg.Any<CancellationToken>())
            .Returns(new StarProgress { IsComplete = true, CompletedLocations = 10, TotalLocations = 10, IncompleteLocations = [] });
        _roleService.HasRoleAsync(TestUserId, TestGuildId, Arg.Any<Role>())
            .Returns(true);

        // Act
        var result = await _orchestrator.ProcessAchievementSilentlyAsync(TestMhid, TestUserId, TestGuildId, TestAchievement);

        // Assert
        Assert.AreEqual(ClaimResult.AlreadyHasRole, result.Result);
        Assert.IsNotNull(result.Progress);
        await _roleService.DidNotReceive().AddRoleAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<Role>(), Arg.Any<CancellationToken>());
        await _achievementMessageService.DidNotReceive().SendAchievementMessageAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<AchievementRole>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task ProcessAchievementSilentlyAsync_WhenUserDoesNotHaveAchievement_ReturnsNotAchieved()
    {
        // Arrange
        _achievementService.HasAchievementAsync(TestMhid, TestAchievement, Arg.Any<CancellationToken>())
            .Returns(new StarProgress { IsComplete = false, CompletedLocations = 5, TotalLocations = 10, IncompleteLocations = ["Location1", "Location2", "Location3", "Location4", "Location5"] });

        // Act
        var result = await _orchestrator.ProcessAchievementSilentlyAsync(TestMhid, TestUserId, TestGuildId, TestAchievement);

        // Assert
        Assert.AreEqual(ClaimResult.NotAchieved, result.Result);
        Assert.IsNotNull(result.Progress);
        Assert.IsFalse(result.Progress.IsComplete);
        await _roleService.DidNotReceive().HasRoleAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<Role>());
        await _roleService.DidNotReceive().AddRoleAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<Role>(), Arg.Any<CancellationToken>());
        await _achievementMessageService.DidNotReceive().SendAchievementMessageAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<AchievementRole>(), Arg.Any<CancellationToken>());
    }

    //[TestMethod]
    //public async Task ResetAchievementAsync_WhenRoleIsConfigured_RemovesRoleFromAllUsersAndAddsAchieverRole()
    //{
    //    // Arrange
    //    _roleService.GetRoleIdAsync(TestGuildId, Arg.Any<Role>())
    //        .Returns(TestRoleId);

    //    var mockCache = Substitute.For<IGatewayClientCache>();
    //    var mockGuild = Substitute.For<Guild>();
    //    var mockUser1 = Substitute.For<GuildUser>();
    //    var mockUser2 = Substitute.For<GuildUser>();

    //    mockUser1.Id.Returns(100uL);
    //    mockUser1.RoleIds.Returns([TestRoleId]);
    //    mockUser2.Id.Returns(200uL);
    //    mockUser2.RoleIds.Returns([TestRoleId]);

    //    var users = new Dictionary<ulong, GuildUser>
    //    {
    //        { 100uL, mockUser1 },
    //        { 200uL, mockUser2 }
    //    };

    //    mockGuild.Users.Returns(users);
    //    mockCache.Guilds.Returns(new Dictionary<ulong, Guild?> { { TestGuildId, mockGuild } });
    //    _gatewayClient.Cache.Returns(mockCache);

    //    var progressCalls = new List<(int current, int total)>();
    //    Task ProgressCallback(int current, int total)
    //    {
    //        progressCalls.Add((current, total));
    //        return Task.CompletedTask;
    //    }

    //    // Act
    //    await _orchestrator.ResetAchievementAsync(TestGuildId, TestAchievement, ProgressCallback);

    //    // Assert
    //    await _roleService.Received(2).AddRoleAsync(Arg.Any<ulong>(), TestGuildId, Role.Achiever, Arg.Any<CancellationToken>());
    //    await _roleService.Received(2).RemoveRoleAsync(Arg.Any<ulong>(), TestGuildId, Arg.Any<Role>(), Arg.Any<CancellationToken>());

    //    Assert.AreEqual(2, progressCalls.Count);
    //    Assert.AreEqual((0, 2), progressCalls[0]);
    //    Assert.AreEqual((1, 2), progressCalls[1]);
    //}

    //[TestMethod]
    //public async Task ResetAchievementAsync_WhenRoleIsNotConfigured_ThrowsRoleNotConfiguredException()
    //{
    //    // Arrange
    //    _roleService.GetRoleIdAsync(TestGuildId, Arg.Any<Role>())
    //        .Returns((ulong?)null);

    //    Task ProgressCallback(int current, int total) => Task.CompletedTask;

    //    // Act & Assert
    //    await Assert.ThrowsExceptionAsync<RoleNotConfiguredException>(
    //        () => _orchestrator.ResetAchievementAsync(TestGuildId, TestAchievement, ProgressCallback));

    //    await _roleService.DidNotReceive().AddRoleAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<Role>(), Arg.Any<CancellationToken>());
    //    await _roleService.DidNotReceive().RemoveRoleAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<Role>(), Arg.Any<CancellationToken>());
    //}

    //[TestMethod]
    //public async Task ResetAchievementAsync_WhenGuildNotInCache_CompletesWithoutError()
    //{
    //    // Arrange
    //    _roleService.GetRoleIdAsync(TestGuildId, Arg.Any<Role>())
    //        .Returns(TestRoleId);

    //    var mockCache = Substitute.For<IGatewayClientCache>();
    //    mockCache.Guilds.Returns(new Dictionary<ulong, Guild?>());
    //    _gatewayClient.Cache.Returns(mockCache);

    //    var progressCalls = new List<(int current, int total)>();
    //    Task ProgressCallback(int current, int total)
    //    {
    //        progressCalls.Add((current, total));
    //        return Task.CompletedTask;
    //    }

    //    // Act
    //    await _orchestrator.ResetAchievementAsync(TestGuildId, TestAchievement, ProgressCallback);

    //    // Assert
    //    await _roleService.DidNotReceive().AddRoleAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<Role>(), Arg.Any<CancellationToken>());
    //    await _roleService.DidNotReceive().RemoveRoleAsync(Arg.Any<ulong>(), Arg.Any<ulong>(), Arg.Any<Role>(), Arg.Any<CancellationToken>());

    //    Assert.AreEqual(1, progressCalls.Count);
    //    Assert.AreEqual((0, 0), progressCalls[0]);
    //}
}
