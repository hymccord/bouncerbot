using System.Collections.ObjectModel;

using BouncerBot.Db;
using BouncerBot.Db.Models;

namespace BouncerBot.Modules.Config;

public class ConfigService(BouncerBotDbContext dbContext)
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

    public async Task SetLogChannelSettingAsync(ulong guildId, LogChannel channel, ulong? channelId)
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
            Role.Achiever => setting?.AchieverId,
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
            case Role.Achiever:
                setting.AchieverId = roleId;
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

    public async Task<string?> GetMessageSettingAsync(ulong guildId, AchievementRole role)
    {
        var setting = await dbContext.AchievementMessages.FindAsync(guildId);
        return role switch
        {
            AchievementRole.Star => setting?.Star,
            AchievementRole.Crown => setting?.Crown,
            AchievementRole.Checkmark => setting?.Checkmark,
            AchievementRole.EggMaster => setting?.EggMaster,
            AchievementRole.ArcaneMaster => setting?.ArcaneMaster,
            AchievementRole.DraconicMaster => setting?.DraconicMaster,
            AchievementRole.ForgottenMaster => setting?.ForgottenMaster,
            AchievementRole.HydroMaster => setting?.HydroMaster,
            AchievementRole.LawMaster => setting?.LawMaster,
            AchievementRole.PhysicalMaster => setting?.PhysicalMaster,
            AchievementRole.RiftMaster => setting?.RiftMaster,
            AchievementRole.ShadowMaster => setting?.ShadowMaster,
            AchievementRole.TacticalMaster => setting?.TacticalMaster,
            AchievementRole.MultiMaster => setting?.MultiMaster,
            _ => null
        };
    }

    public async Task SetMessageSettingAsync(ulong guildId, AchievementRole role, string message)
    {
        var setting = await dbContext.AchievementMessages.FindAsync(guildId);
        if (setting is null)
        {
            setting = new AchievementMessage { GuildId = guildId };
            dbContext.AchievementMessages.Add(setting);
        }

        switch (role)
        {
            case AchievementRole.Star:
                setting.Star = message;
                break;
            case AchievementRole.Crown:
                setting.Crown = message;
                break;
            case AchievementRole.Checkmark:
                setting.Checkmark = message;
                break;
            case AchievementRole.EggMaster:
                setting.EggMaster = message;
                break;
            case AchievementRole.ArcaneMaster:
                setting.ArcaneMaster = message;
                break;
            case AchievementRole.DraconicMaster:
                setting.DraconicMaster = message;
                break;
            case AchievementRole.ForgottenMaster:
                setting.ForgottenMaster = message;
                break;
            case AchievementRole.HydroMaster:
                setting.HydroMaster = message;
                break;
            case AchievementRole.LawMaster:
                setting.LawMaster = message;
                break;
            case AchievementRole.PhysicalMaster:
                setting.PhysicalMaster = message;
                break;
            case AchievementRole.RiftMaster:
                setting.RiftMaster = message;
                break;
            case AchievementRole.ShadowMaster:
                setting.ShadowMaster = message;
                break;
            case AchievementRole.TacticalMaster:
                setting.TacticalMaster = message;
                break;
            case AchievementRole.MultiMaster:
                setting.MultiMaster = message;
                break;
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<(LogSetting? LogSettings, RoleSetting? RoleSettings, AchievementMessage? AchievementMessages)> GetGuildConfigAsync(ulong guildId)
    {
        var logSetting = await dbContext.LogSettings.FindAsync(guildId);
        var roleSetting = await dbContext.RoleSettings.FindAsync(guildId);
        var messageSetting = await dbContext.AchievementMessages.FindAsync(guildId);

        return (logSetting, roleSetting, messageSetting);
    }
}
