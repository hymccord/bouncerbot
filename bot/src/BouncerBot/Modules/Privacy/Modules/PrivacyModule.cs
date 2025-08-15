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
            **Privacy Policy**

            **Information We Collect:**
            • Discord user information (Discord User ID)
            • MouseHunt ID when your account is verified

            **How We Use Your Information:**
            • To provide bot achievement functionality
            • Moderators can view Discord user IDs and MouseHunt IDs for server moderation purposes

            **Data Retention:**
            • We **permanently retain** anonymous hashes of the Discord ID and MouseHunt ID after verification
            • Discord ID and MouseHunt ID are stored together while your account remains verified
            • When you {commandMentionService.GetCommandMention(VerifyModuleMetadata.UnverifyCommand.Name)} or leave the server (willingly or not), all identifiable verified information is deleted

            **Data Protection:**
            • Your information is stored on the secure <https://mhct.win> server (right next to LarryBot) and only used for bot operations
            • You can {commandMentionService.GetCommandMention(VerifyModuleMetadata.UnverifyCommand.Name)} at any time

            **Contact:**
            If you have questions about this privacy policy or your data, please contact the moderators.

            For any technical questions, contact my developer: <@148604445800923137>

            **Source Code:**
            [GitHub](https://www.github.com/hymccord/bouncerbot/)
            """,
            Flags = NetCord.MessageFlags.Ephemeral
        }));
    }
}
