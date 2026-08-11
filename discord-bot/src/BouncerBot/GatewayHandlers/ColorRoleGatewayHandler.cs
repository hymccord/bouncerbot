using BouncerBot.Modules.Achieve;
using BouncerBot.Services;
using Microsoft.Extensions.DependencyInjection;
using NetCord;
using NetCord.Gateway;
using DiscordRole = NetCord.Role;
using NetCord.Hosting.Gateway;
using BouncerBot.Modules.ColorMe;

namespace BouncerBot.GatewayHandlers;

public sealed class ColorRoleGatewayHandler(
    IColorRoleRegistry colorRoleRegistry,
    GatewayClient client,
    IServiceScopeFactory scopeFactory) :
    IGuildCreateGatewayHandler,
    IGuildUpdateGatewayHandler,
    IRoleCreateGatewayHandler,
    IRoleUpdateGatewayHandler,
    IRoleDeleteGatewayHandler,
    IGuildUserUpdateGatewayHandler
{
    ValueTask IGuildCreateGatewayHandler.HandleAsync(GuildCreateEventArgs arg)
    {
        if (arg.Guild is not null)
        {
            colorRoleRegistry.UpdateGuild(arg.Guild);
        }

        return ValueTask.CompletedTask;
    }

    ValueTask IGuildUpdateGatewayHandler.HandleAsync(Guild guild)
    {
        colorRoleRegistry.UpdateGuild(guild);
        return ValueTask.CompletedTask;
    }

    ValueTask IRoleCreateGatewayHandler.HandleAsync(DiscordRole role)
    {
        colorRoleRegistry.UpdateRole(role);
        return ValueTask.CompletedTask;
    }

    ValueTask IRoleUpdateGatewayHandler.HandleAsync(DiscordRole role)
    {
        colorRoleRegistry.UpdateRole(role);
        return ValueTask.CompletedTask;
    }

    ValueTask IRoleDeleteGatewayHandler.HandleAsync(RoleDeleteEventArgs role)
    {
        colorRoleRegistry.RemoveRole(role);
        return ValueTask.CompletedTask;
    }

    async ValueTask IGuildUserUpdateGatewayHandler.HandleAsync(GuildUser guildUser)
    {
        if (!client.Cache.Guilds.TryGetValue(guildUser.GuildId, out var guild)
            || !guild.Users.TryGetValue(guildUser.Id, out var oldUser))
        {
            return;
        }

        var removedRoleIds = oldUser.RoleIds.Except(guildUser.RoleIds).ToHashSet();
        if (removedRoleIds.Count == 0)
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();

        foreach (var (achievement, powerType) in s_masteryRoles)
        {
            ulong masteryRoleId;
            try
            {
                masteryRoleId = await roleService.GetRoleIdAsync(guildUser.GuildId, achievement)
                    ?? throw new RoleNotConfiguredException(achievement);
            }
            catch (RoleNotConfiguredException)
            {
                continue;
            }

            if (!removedRoleIds.Contains(masteryRoleId)
                || guild.Roles.Values.FirstOrDefault(role =>
                    string.Equals(role.Name, $"ColorMe{powerType}", StringComparison.OrdinalIgnoreCase)) is not { } colorRole
                || !guildUser.RoleIds.Contains(colorRole.Id))
            {
                continue;
            }

            await guildUser.RemoveRoleAsync(colorRole.Id);
        }
    }

    private static readonly IReadOnlyDictionary<Role, PowerType> s_masteryRoles = new Dictionary<Role, PowerType>
    {
        [Role.ArcaneMaster] = PowerType.Arcane,
        [Role.DraconicMaster] = PowerType.Draconic,
        [Role.ForgottenMaster] = PowerType.Forgotten,
        [Role.HydroMaster] = PowerType.Hydro,
        [Role.LawMaster] = PowerType.Law,
        [Role.PhysicalMaster] = PowerType.Physical,
        [Role.RiftMaster] = PowerType.Rift,
        [Role.ShadowMaster] = PowerType.Shadow,
        [Role.TacticalMaster] = PowerType.Tactical,
        [Role.MultiMaster] = PowerType.Multi,
    };
}
