using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MonstroBot.Db;
using MonstroBot.Db.Models;

using NetCord.Services;

namespace MonstroBot.Attributes;
internal class RequireVerificationStatusAttribute<TContext> : PreconditionAttribute<TContext>
    where TContext : IUserContext, IGuildContext
{
    private readonly VerificationStatus _verificationStatus;

    public RequireVerificationStatusAttribute(VerificationStatus verificationStatus)
    {
        _verificationStatus = verificationStatus;
    }

    public override async ValueTask<PreconditionResult> EnsureCanExecuteAsync(TContext context, IServiceProvider? serviceProvider)
    {
        if (serviceProvider is null)
        {
            return PreconditionResult.Fail("The bot encountered an unexpected error.");
        }

        if (context.Guild is null)
        {
            return PreconditionResult.Fail("The current guild could not be found.");
        }

        var userId = context.User.Id;
        var guildId = context.Guild.Id;

        using var dbContext = serviceProvider!.GetRequiredService<MonstroBotDbContext>();

        VerifiedUser? user = await dbContext.VerifiedUsers
            .FirstOrDefaultAsync(vu => vu.DiscordId == userId && vu.GuildId == guildId);

        return (user, _verificationStatus) switch
        {
            (null, VerificationStatus.Unverified) => PreconditionResult.Success,
            (null, VerificationStatus.Verified) => PreconditionResult.Fail("You must be verified to use this command."),
            (_, VerificationStatus.Unverified) => PreconditionResult.Fail("You are already verified."),
            (_, VerificationStatus.Verified) => PreconditionResult.Success,
            _ => PreconditionResult.Fail("Unexpected verification status.")
        };
    }
}
