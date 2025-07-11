using Humanizer;

using BouncerBot.Attributes;
using BouncerBot.Db;

using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Variables.Modules;

[SlashCommand("config", "Manage bot configuration")]
[RequireUserPermissions<ApplicationCommandContext>(Permissions.ManageGuild)]
[GuildOnly<ApplicationCommandContext>]
public class ConfigModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("aio", "Configure everything interactively!")]
    public async Task AioAsync()
    {

    }

    [SubSlashCommand("log", "Set ")]
    public async Task SetChannelVariables()
    {
        //await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        {
            Embeds = [
                new EmbedProperties() {
                    Title = "Select a channel setting to change"
                }
            ],
            Components = [
               new ActionRowProperties().WithButtons([
                   new ButtonProperties($"variables channels:{(int)LogChannel.General}", "General", ButtonStyle.Secondary),
                   new ButtonProperties($"variables channels:{(int)LogChannel.Achievement}", "Achievement", ButtonStyle.Secondary),
                   new ButtonProperties($"variables channels:{(int)LogChannel.EggMaster}", "Egg Master", ButtonStyle.Secondary),
                   new ButtonProperties($"variables channels:{(int)LogChannel.Verification}", "Verification", ButtonStyle.Secondary)
               ]),
            ],
            Flags = MessageFlags.Ephemeral
        }));
    }

    [SubSlashCommand("roles", "Set server roles")]
    public async Task SetUserVariables()
    {

        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        {
            Embeds = [
                new EmbedProperties() {
                    Title = "Select a role setting to change"
                }
            ],
            Components = [
               new ActionRowProperties().WithButtons([
                   new ButtonProperties($"variables roles:{(int)Role.Star}", new EmojiProperties("⭐"), ButtonStyle.Secondary),
                   new ButtonProperties($"variables roles:{(int)Role.Crown}", new EmojiProperties("👑"), ButtonStyle.Secondary),
                   new ButtonProperties($"variables roles:{(int)Role.Checkmark}", new EmojiProperties("✅"), ButtonStyle.Secondary),
                   new ButtonProperties($"variables roles:{(int)Role.EggMaster}", new EmojiProperties("🥚"), ButtonStyle.Secondary),
               ]),
               new ActionRowProperties().WithButtons([
                   new ButtonProperties($"variables roles:{(int)Role.ArcaneMaster}", Role.ArcaneMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables roles:{(int)Role.DraconicMaster}", Role.DraconicMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables roles:{(int)Role.ForgottenMaster}", Role.ForgottenMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables roles:{(int)Role.HydroMaster}", Role.HydroMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables roles:{(int)Role.LawMaster}", Role.LawMaster.Humanize(), ButtonStyle.Secondary),
               ]),
               new ActionRowProperties().WithButtons([
                   new ButtonProperties($"variables roles:{(int)Role.PhysicalMaster}", Role.PhysicalMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables roles:{(int)Role.RiftMaster}", Role.RiftMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables roles:{(int)Role.ShadowMaster}", Role.ShadowMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables roles:{(int)Role.TacticalMaster}", Role.TacticalMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables roles:{(int)Role.MultiMaster}", Role.MultiMaster.Humanize(), ButtonStyle.Secondary),
               ]),
               new ActionRowProperties().WithButtons([
                   new ButtonProperties($"variables roles:{(int)Role.TradeBanned}", Role.TradeBanned.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables roles:{(int)Role.MapBanned}", Role.MapBanned.Humanize(), ButtonStyle.Secondary),
                ])
            ],
            Flags = MessageFlags.Ephemeral
        }));
    }

    [SubSlashCommand("messages", "Set achievement messages")]
    public async Task SetMessageVariables()
    {
        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        {
            Embeds = [
                new EmbedProperties() {
                    Title = "Select a message setting to change"
                }
            ],
            Components = [
               new ActionRowProperties().WithButtons([
                   new ButtonProperties($"variables messages:{(int)Role.Star}", new EmojiProperties("⭐"), ButtonStyle.Secondary),
                   new ButtonProperties($"variables messages:{(int)Role.Crown}", new EmojiProperties("👑"), ButtonStyle.Secondary),
                   new ButtonProperties($"variables messages:{(int)Role.Checkmark}", new EmojiProperties("✅"), ButtonStyle.Secondary),
                   new ButtonProperties($"variables messages:{(int)Role.EggMaster}", new EmojiProperties("🥚"), ButtonStyle.Secondary),
               ]),
               new ActionRowProperties().WithButtons([
                   new ButtonProperties($"variables messages:{(int)Role.ArcaneMaster}", Role.ArcaneMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables messages:{(int)Role.DraconicMaster}", Role.DraconicMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables messages:{(int)Role.ForgottenMaster}", Role.ForgottenMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables messages:{(int)Role.HydroMaster}", Role.HydroMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables messages:{(int)Role.LawMaster}", Role.LawMaster.Humanize(), ButtonStyle.Secondary),
               ]),
               new ActionRowProperties().WithButtons([
                   new ButtonProperties($"variables messages:{(int)Role.PhysicalMaster}", Role.PhysicalMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables messages:{(int)Role.RiftMaster}", Role.RiftMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables messages:{(int)Role.ShadowMaster}", Role.ShadowMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables messages:{(int)Role.TacticalMaster}", Role.TacticalMaster.Humanize(), ButtonStyle.Secondary),
                   new ButtonProperties($"variables messages:{(int)Role.MultiMaster}", Role.MultiMaster.Humanize(), ButtonStyle.Secondary),
               ]),
            ],
            Flags = MessageFlags.Ephemeral
        }));
    }
}

