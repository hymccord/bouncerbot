using BouncerBot.Attributes;

using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace BouncerBot.Modules.Privacy.Modules;
[RequireGuildContext<ApplicationCommandContext>]
public class PrivacyModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("privacy", "Show the privacy policy")]
    public async Task ShowPrivacyPolicyAsync()
    {
        await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties()
        {
            Content = """
            **Privacy Policy**

            **Information We Collect:**
            • Discord user information (user ID)
            • Hunter ID when your account is linked to our service
            * 

            **How We Use Your Information:**
            • To provide bot functionality and maintain account links
            • To verify and manage your hunter profile

            **Data Retention:**
            • Discord ID and Hunter ID information is stored while your account remains linked
            • When you unlink your account, all identifiable information is deleted
            • We retain anonymous hashes to detect if a different hunter ID is used for a given Discord user

            **Data Protection:**
            • Your information is stored securely and only used for bot operations
            • We do not share your personal information with third parties
            • You can unlink at any time

            **Contact:**
            If you have questions about this privacy policy or your data, please contact the moderators.

            For any technical questions, contact the bot owner/developer: <@148604445800923137>
            """,
            Flags = NetCord.MessageFlags.Ephemeral
        }));
    }
}
