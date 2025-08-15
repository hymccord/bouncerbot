using BouncerBot.Db;
using BouncerBot.Modules.Verification;
using BouncerBot.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NetCord.Services;

namespace BouncerBot.Attributes;
internal class RequireVerificationStatusAttribute<TContext> : RequireContextAttribute<TContext>
    where TContext : IUserContext, IGuildContext
{
    private readonly VerificationStatus _shouldBe;

    public RequireVerificationStatusAttribute(VerificationStatus verificationStatus)
        : base(RequiredContext.Guild)
    {
        _shouldBe = verificationStatus;
    }

    public override async ValueTask<PreconditionResult> EnsureCanExecuteAsync(TContext context, IServiceProvider? serviceProvider)
    {
        if (serviceProvider is null)
        {
            return PreconditionResult.Fail("The bot encountered an unexpected error.");
        }

        var userId = context.User.Id;
        var guildId = context.Guild!.Id;

        var dbContext = serviceProvider.GetRequiredService<BouncerBotDbContext>();
        var commandMentionService = serviceProvider.GetRequiredService<ICommandMentionService>();

        var isVerified = await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.DiscordId == userId && vu.GuildId == guildId) is not null;

        return (isVerified, _shouldBe) switch
        {
            (false, VerificationStatus.Unverified) => PreconditionResult.Success,
            (false, VerificationStatus.Verified) => PreconditionResult.Fail($""""
                This is a verified user only club! Once you're on my list, I'll let you use that command.

                -# Hint: Use the {commandMentionService.GetCommandMention("link")} command.
                """"),
            (true, VerificationStatus.Unverified) => PreconditionResult.Fail($"""
                I don't let verified users use this! I could bounce you out of my club, then I'll let you use that command.
                
                -# Hint: Use the {commandMentionService.GetCommandMention("unlink")} command.
                """),
            (true, VerificationStatus.Verified) => PreconditionResult.Success,

            _ => PreconditionResult.Fail("Unexpected verification status.")
        };
    }
}
