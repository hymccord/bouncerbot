using BouncerBot.Db;
using BouncerBot.Db.Models;

namespace BouncerBot.Services;

public class VariablesService(BouncerBotDbContext dbContext)
{
    public async Task<ulong?> GetChannelSettingAsync(ulong guildId, LogChannel channel)
    {
        var setting = await dbContext.LogSettings.FindAsync(guildId);
        return channel switch
        {
            LogChannel.General => setting?.LogId,
            LogChannel.Achievement => setting?.FlexId,
            LogChannel.EggMaster => setting?.EggMasterId,
            LogChannel.Verification => setting?.VerificationId,
            _ => null
        };
    }

    public async Task SetChannelSettingAsync(ulong guildId, LogChannel channel, ulong channelId)
    {
        var setting = await dbContext.LogSettings.FindAsync(guildId);
        if (setting is null)
        {
            setting = new LogSetting { GuildId = guildId };
            dbContext.LogSettings.Add(setting);
        }

        switch (channel)
        {
            case LogChannel.General:
                setting.LogId = channelId;
                break;
            case LogChannel.Achievement:
                setting.FlexId = channelId;
                break;
            case LogChannel.EggMaster:
                setting.EggMasterId = channelId;
                break;
            case LogChannel.Verification:
                setting.VerificationId = channelId;
                break;
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<ulong?> GetRoleSettingAsync(ulong guildId, Role role)
    {
        var setting = await dbContext.RoleSettings.FindAsync(guildId);
        return role switch
        {
            Role.Star => setting?.StarId,
            Role.Crown => setting?.CrownId,
            Role.Checkmark => setting?.CheckmarkId,
            Role.EggMaster => setting?.EggMasterId,
            Role.ArcaneMaster => setting?.ArcaneMasterId,
            Role.DraconicMaster => setting?.DraconicMasterId,
            Role.ForgottenMaster => setting?.ForgottenMasterId,
            Role.HydroMaster => setting?.HydroMasterId,
            Role.LawMaster => setting?.LawMasterId,
            Role.PhysicalMaster => setting?.PhysicalMasterId,
            Role.RiftMaster => setting?.RiftMasterId,
            Role.ShadowMaster => setting?.ShadowMasterId,
            Role.TacticalMaster => setting?.TacticalMasterId,
            Role.MultiMaster => setting?.MultiMasterId,
            Role.Verified => setting?.VerifiedId,
            Role.MapBanned => setting?.MapBannedId,
            Role.TradeBanned => setting?.TradeBannedId,
            _ => null
        };
    }

    public async Task SetRoleSettingAsync(ulong guildId, Role role, ulong roleId)
    {
        var setting = await dbContext.RoleSettings.FindAsync(guildId);
        if (setting is null)
        {
            setting = new RoleSetting { GuildId = guildId };
            dbContext.RoleSettings.Add(setting);
        }

        switch (role)
        {
            case Role.Star:
                setting.StarId = roleId;
                break;
            case Role.Crown:
                setting.CrownId = roleId;
                break;
            case Role.Checkmark:
                setting.CheckmarkId = roleId;
                break;
            case Role.EggMaster:
                setting.EggMasterId = roleId;
                break;
            case Role.ArcaneMaster:
                setting.ArcaneMasterId = roleId;
                break;
            case Role.DraconicMaster:
                setting.DraconicMasterId = roleId;
                break;
            case Role.ForgottenMaster:
                setting.ForgottenMasterId = roleId;
                break;
            case Role.HydroMaster:
                setting.HydroMasterId = roleId;
                break;
            case Role.LawMaster:
                setting.LawMasterId = roleId;
                break;
            case Role.PhysicalMaster:
                setting.PhysicalMasterId = roleId;
                break;
            case Role.RiftMaster:
                setting.RiftMasterId = roleId;
                break;
            case Role.ShadowMaster:
                setting.ShadowMasterId = roleId;
                break;
            case Role.TacticalMaster:
                setting.TacticalMasterId = roleId;
                break;
            case Role.MultiMaster:
                setting.MultiMasterId = roleId;
                break;
            case Role.Verified:
                setting.VerifiedId = roleId;
                break;
            case Role.MapBanned:
                setting.MapBannedId = roleId;
                break;
            case Role.TradeBanned:
                setting.TradeBannedId = roleId;
                break;
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<string?> GetMessageSettingAsync(ulong guildId, Role role)
    {
        var setting = await dbContext.AchievementMessages.FindAsync(guildId);
        return role switch
        {
            Role.Star => setting?.Star,
            Role.Crown => setting?.Crown,
            Role.Checkmark => setting?.Checkmark,
            Role.EggMaster => setting?.EggMaster,
            Role.ArcaneMaster => setting?.ArcaneMaster,
            Role.DraconicMaster => setting?.DraconicMaster,
            Role.ForgottenMaster => setting?.ForgottenMaster,
            Role.HydroMaster => setting?.HydroMaster,
            Role.LawMaster => setting?.LawMaster,
            Role.PhysicalMaster => setting?.PhysicalMaster,
            Role.RiftMaster => setting?.RiftMaster,
            Role.ShadowMaster => setting?.ShadowMaster,
            Role.TacticalMaster => setting?.TacticalMaster,
            Role.MultiMaster => setting?.MultiMaster,
            _ => null
        };
    }

    public async Task SetMessageSettingAsync(ulong guildId, Role role, string message)
    {
        var setting = await dbContext.AchievementMessages.FindAsync(guildId);
        if (setting is null)
        {
            setting = new AchievementMessage { GuildId = guildId };
            dbContext.AchievementMessages.Add(setting);
        }

        switch (role)
        {
            case Role.Star:
                setting.Star = message;
                break;
            case Role.Crown:
                setting.Crown = message;
                break;
            case Role.Checkmark:
                setting.Checkmark = message;
                break;
            case Role.EggMaster:
                setting.EggMaster = message;
                break;
            case Role.ArcaneMaster:
                setting.ArcaneMaster = message;
                break;
            case Role.DraconicMaster:
                setting.DraconicMaster = message;
                break;
            case Role.ForgottenMaster:
                setting.ForgottenMaster = message;
                break;
            case Role.HydroMaster:
                setting.HydroMaster = message;
                break;
            case Role.LawMaster:
                setting.LawMaster = message;
                break;
            case Role.PhysicalMaster:
                setting.PhysicalMaster = message;
                break;
            case Role.RiftMaster:
                setting.RiftMaster = message;
                break;
            case Role.ShadowMaster:
                setting.ShadowMaster = message;
                break;
            case Role.TacticalMaster:
                setting.TacticalMaster = message;
                break;
            case Role.MultiMaster:
                setting.MultiMaster = message;
                break;
        }

        await dbContext.SaveChangesAsync();
    }
}
