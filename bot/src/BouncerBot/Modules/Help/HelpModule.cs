using BouncerBot.Attributes;
using BouncerBot.Modules.Achieve.Modules;
using BouncerBot.Modules.Bounce.Modules;
using BouncerBot.Modules.Claim.Modules;
using BouncerBot.Modules.Config.Modules;
using BouncerBot.Modules.Privacy.Modules;
using BouncerBot.Modules.Verify.Modules;
using BouncerBot.Modules.WhoIs.Modules;
using BouncerBot.Services;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Help;


public class HelpModule(
    ICommandMentionService cms
    ) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("help", "Provides information about the bot and its commands.")]
    [RequireGuildContext<ApplicationCommandContext>]
    public async Task HelpAsync()
    {
        if (Context.User is GuildInteractionUser user)
        {
            var perms = user.Permissions;

            List<EmbedProperties> embeds = [];

            embeds.Add(new EmbedProperties()
            {
                Title = "User Commands",
                Description = $"""
                - {cms.GetCommandMention(VerifyModuleMetadata.VerifyCommand.Name)} `<hunterId>`: {VerifyModuleMetadata.VerifyCommand.Description}
                - {cms.GetCommandMention(VerifyModuleMetadata.UnverifyCommand.Name)}: {VerifyModuleMetadata.UnverifyCommand.Name}
                - {cms.GetCommandMention(ClaimModuleMetadata.ClaimCommand.Name)} `<achievement> [private]`: {ClaimModuleMetadata.ClaimCommand.Description}
                - {cms.GetCommandMention(PrivacyModuleMetadata.PrivacyCommand.Name)}: {PrivacyModuleMetadata.PrivacyCommand.Description}
                - {cms.GetCommandMention(HelpModuleMetadata.HelpCommand.Name)}: {HelpModuleMetadata.HelpCommand.Description}
                """
            });

            if ((perms & Permissions.ManageRoles) > 0)
            {
                embeds.Add(new EmbedProperties()
                {
                    Title = "Moderator Commands",
                    Description = $"""
                    - {cms.GetSubCommandMention("achieve verify")} `<hunterId> <achievement>`: {AchieveModuleMetadata.VerifyCommand.Description}
                    - {cms.GetSubCommandMention("achieve reset")} `<achievement>`: {AchieveModuleMetadata.ResetCommand.Description}
                    - {cms.GetSubCommandMention("bounce add")} `<hunterId> [note]`: {BounceModuleMetadata.AddCommand.Description}
                    - {cms.GetSubCommandMention("bounce remove")} `<hunterId>`: {BounceModuleMetadata.RemoveCommand.Description}
                    - {cms.GetSubCommandMention("bounce remove-all")}: {BounceModuleMetadata.RemoveAllCommand.Description}
                    - {cms.GetSubCommandMention("bounce list")}: {BounceModuleMetadata.ListCommand.Description}
                    - {cms.GetSubCommandMention("bounce check")} `<hunterId>`: {BounceModuleMetadata.CheckCommand.Description}
                    - {cms.GetSubCommandMention("bounce note")} `<hunterId> [note]`: {BounceModuleMetadata.NoteCommand.Description}
                    - {cms.GetSubCommandMention("whois user")} `<user>`: {WhoIsModuleMetadata.UserCommand.Description}
                    - {cms.GetSubCommandMention("whois hunter")} `<hunterId>`: {WhoIsModuleMetadata.HunterCommand.Description}
                    """,
                });
            }

            if ((perms & Permissions.ManageGuild) > 0)
            {
                embeds.Add(new EmbedProperties()
                {
                    Title = "Administrator Commands",
                    Description = $"""
                    - {cms.GetSubCommandMention("config log")} `<type> [channel]`: {ConfigModuleMetadata.LogCommand.Description}
                    - {cms.GetSubCommandMention("config role")} `<role> <selectedRole>`: {ConfigModuleMetadata.RoleCommand.Description}
                    - {cms.GetSubCommandMention("config message")} `<achievement> <message>`: {ConfigModuleMetadata.MessageCommand.Description}
                    - {cms.GetSubCommandMention("config verify")} `<min_rank>`: {ConfigModuleMetadata.VerifyCommand.Description}
                    - {cms.GetSubCommandMention("config list")} `[setting]`: {ConfigModuleMetadata.ListCommand.Description}
                    """,
                });
            }

            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            {
                Content = "Here are the commands you have access to:",
                Embeds = embeds,
                Flags = MessageFlags.Ephemeral
            }));
        }
        else
        {
            await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
            {
                Content = "Unable to determine your Discord permissions to see what commands you have access to.",
                Flags = MessageFlags.Ephemeral
            }));
        }
    }
}
