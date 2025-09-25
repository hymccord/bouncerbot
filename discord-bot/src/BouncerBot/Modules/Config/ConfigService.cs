
using BouncerBot.Db;
using BouncerBot.Db.Models;

using Microsoft.EntityFrameworkCore;

namespace BouncerBot.Modules.Config;

public interface IConfigService
{
    Task<ulong?> GetLogChannelAsync(ulong guildId, LogChannel channel);
    Task<ulong?> GetAchievementLogChannelAsync(ulong guildId, AchievementRole achievementRole);
    Task<string?> GetAchievementMessageAsync(ulong guildId, AchievementRole role);
    Task<ulong?> GetRoleIdAsync(ulong guildId, Role role);
    Task<Rank> GetVerifyRankAsync(ulong guildId);

    Task SetLogChannelSettingAsync(ulong guildId, LogChannel channel, ulong? channelId);
    Task SetAchievementLogChannelAsync(ulong guildId, AchievementRole achievementRole, ulong? channelId);
    Task SetAchievementMessageAsync(ulong guildId, AchievementRole role, string message);
    Task SetRoleIdAsync(ulong guildId, Role role, ulong roleId);
    Task SetVerifyRankAsync(ulong id, Rank minRank);
}

public class ConfigService(BouncerBotDbContext dbContext) : IConfigService
{
    public async Task<ulong?> GetLogChannelAsync(ulong guildId, LogChannel channel)
    {
        var setting = await dbContext.LogSettings.FindAsync(guildId);
        return channel switch
        {
            LogChannel.General => setting?.GeneralId,
            LogChannel.Achievement => setting?.AchievementId,
            LogChannel.Verification => setting?.VerificationId,
            _ => null
        };
    }

    public async Task SetLogChannelSettingAsync(ulong guildId, LogChannel channel, ulong? channelId)
    {
        var setting = await dbContext.LogSettings.FindAsync(guildId);
        if (setting is null)
        {
            setting = new Db.Models.LogChannelsSetting { GuildId = guildId };
            dbContext.LogSettings.Add(setting);
        }

        switch (channel)
        {
            case LogChannel.General:
                setting.GeneralId = channelId;
                break;
            case LogChannel.Achievement:
                setting.AchievementId = channelId;
                break;
            case LogChannel.Verification:
                setting.VerificationId = channelId;
                break;
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<ulong?> GetRoleIdAsync(ulong guildId, Role role)
        => (await dbContext
                .RoleSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(rs => rs.GuildId == guildId && rs.Role == role))?.DiscordRoleId;

    public async Task SetRoleIdAsync(ulong guildId, Role role, ulong roleId)
    {
        var setting = await dbContext.RoleSettings.FirstOrDefaultAsync(rs => rs.GuildId == guildId && rs.Role == role);
        if (setting is null)
        {
            setting = new RoleSetting
            {
                GuildId = guildId,
                Role = role,
                DiscordRoleId = roleId
            };
            dbContext.RoleSettings.Add(setting);
        }
        else
        {
            setting.DiscordRoleId = roleId;
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<string?> GetAchievementMessageAsync(ulong guildId, AchievementRole role)
    {
        return (await dbContext.AchievementMessages
                    .AsNoTracking()
                    .FirstOrDefaultAsync(am => am.GuildId == guildId && am.AchievementRole == role))
                    ?.Message;
    }

    public async Task SetAchievementMessageAsync(ulong guildId, AchievementRole role, string message)
    {
        var setting = await dbContext.AchievementMessages.FirstOrDefaultAsync(am => am.GuildId == guildId && am.AchievementRole == role);
        if (setting is null)
        {
            setting = new AchievementMessage
            {
                GuildId = guildId,
                AchievementRole = role,
                Message = message
            };
            dbContext.AchievementMessages.Add(setting);
        }
        else
        {
            setting.Message = message;
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<Rank> GetVerifyRankAsync(ulong guildId)
    {
        var setting = await dbContext.VerifySettings.FindAsync(guildId);
        return setting?.MinimumRank ?? Rank.Apprentice;
    }

    public async Task SetVerifyRankAsync(ulong id, Rank minRank)
    {
        var setting = await dbContext.VerifySettings.FindAsync(id);
        if (setting is null)
        {
            setting = new VerifySetting { GuildId = id };
            dbContext.VerifySettings.Add(setting);
        }
        setting.MinimumRank = minRank;
        await dbContext.SaveChangesAsync();
    }

    public async Task<ulong?> GetAchievementLogChannelAsync(ulong guildId, AchievementRole achievementRole)
    {
        var achievementOverride = await dbContext.AchievementLogOverrides
            .FirstOrDefaultAsync(alo => alo.GuildId == guildId && alo.AchievementRole == achievementRole);

        return achievementOverride?.ChannelId;
    }

    public async Task SetAchievementLogChannelAsync(ulong guildId, AchievementRole achievementRole, ulong? channelId = null)
    {
        var existing = await dbContext.AchievementLogOverrides
            .FirstOrDefaultAsync(alo => alo.GuildId == guildId && alo.AchievementRole == achievementRole);

        if (channelId.HasValue)
        {
            if (existing == null)
            {
                dbContext.AchievementLogOverrides.Add(new AchievementLogChannelOverride
                {
                    GuildId = guildId,
                    AchievementRole = achievementRole,
                    ChannelId = channelId.Value
                });
            }
            else
            {
                existing.ChannelId = channelId.Value;
            }
        }
        else if (existing != null)
        {
            dbContext.AchievementLogOverrides.Remove(existing);
        }

        await dbContext.SaveChangesAsync();
    }
}
