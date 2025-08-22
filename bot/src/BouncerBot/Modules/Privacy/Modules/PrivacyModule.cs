using BouncerBot.Attributes;
using BouncerBot.Modules.Verify.Modules;
using BouncerBot.Services;

using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Privacy.Modules;
[RequireGuildContext<ApplicationCommandContext>]
public class PrivacyModule(
    ICommandMentionService commandMentionService
) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand(PrivacyModuleMetadata.PrivacyCommand.Name, PrivacyModuleMetadata.PrivacyCommand.Description)]
    public async Task ShowPrivacyPolicyAsync()
    {
        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        {
            Content = $"""
            # Privacy Policy

            ## Information We Collect:
            • Discord User ID
            • MouseHunt Profile ID

            ## How We Use Your Information:
            • Achievement validation
            • Moderators can view Discord User IDs and MouseHunt IDs for server moderation purposes

            ## Data Retention:
            • We **permanently retain** [pseudonymized](<https://en.wikipedia.org/wiki/Pseudonymization>) versions of your Discord ID and MouseHunt ID when you successfully {commandMentionService.GetCommandMention(VerifyModuleMetadata.VerifyCommand.Name)}
            • Discord ID and MouseHunt ID are stored together **while you are verified**
            • When you {commandMentionService.GetCommandMention(VerifyModuleMetadata.UnverifyCommand.Name)} or leave the server (willingly or not), all identifiable information is removed

            ## Data Protection:
            • Your information is stored securely on the <https://mhct.win> server (alongside my buddy <@339098320549052416>) and only used for bot operations

            ## Control Your Privacy:
            • You can {commandMentionService.GetCommandMention(VerifyModuleMetadata.UnverifyCommand.Name)} at any time to remove your data

            ## Contact:
            • Questions about this privacy policy or your data? Please contact the moderators
            • Technical questions, contact my developer: <@148604445800923137>

            ### Source Code:
            [BouncerBot GitHub](<https://www.github.com/hymccord/bouncerbot/>)
            """,
            AllowedMentions = AllowedMentionsProperties.None,
            Flags = NetCord.MessageFlags.Ephemeral
        }));
    }
}
