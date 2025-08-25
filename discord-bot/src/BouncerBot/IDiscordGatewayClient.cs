using NetCord.Gateway;
using NetCord.Rest;

namespace BouncerBot;

public interface IDiscordGatewayClient
{
    IGatewayClientCache Cache { get; }
}

public class DiscordGatewayClient(GatewayClient client) : IDiscordGatewayClient
{
    public IGatewayClientCache Cache => client.Cache;
}

public interface IDiscordRestClient
{
    /// <summary>
    /// Adds a role to a guild user
    /// </summary>
    /// <param name="guildId">The ID of the guild</param>
    /// <param name="userId">The ID of the user</param>
    /// <param name="roleId">The ID of the role to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddGuildUserRoleAsync(ulong guildId, ulong userId, ulong roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a role from a guild user
    /// </summary>
    /// <param name="guildId">The ID of the guild</param>
    /// <param name="userId">The ID of the user</param>
    /// <param name="roleId">The ID of the role to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task RemoveGuildUserRoleAsync(ulong guildId, ulong userId, ulong roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a message from a channel
    /// </summary>
    /// <param name="channelId">The ID of the channel</param>
    /// <param name="messageId">The ID of the message to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteMessageAsync(ulong channelId, ulong messageId, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="DiscordRestClient.SendMessageAsync(ulong, MessageProperties, CancellationToken)"/>
    Task<RestMessage> SendMessageAsync(ulong channelId, MessageProperties message, CancellationToken cancellationToken = default);
}

public class DiscordRestClient(RestClient client) : IDiscordRestClient
{
    public Task AddGuildUserRoleAsync(ulong guildId, ulong userId, ulong roleId, CancellationToken cancellationToken = default)
        => client.AddGuildUserRoleAsync(guildId, userId, roleId, cancellationToken: cancellationToken);
    public Task RemoveGuildUserRoleAsync(ulong guildId, ulong userId, ulong roleId, CancellationToken cancellationToken = default)
        => client.RemoveGuildUserRoleAsync(guildId, userId, roleId, cancellationToken: cancellationToken);
    public Task DeleteMessageAsync(ulong channelId, ulong messageId, CancellationToken cancellationToken = default)
        => client.DeleteMessageAsync(channelId, messageId, cancellationToken: cancellationToken);
    public Task<RestMessage> SendMessageAsync(ulong channelId, MessageProperties message, CancellationToken cancellationToken = default)
        => client.SendMessageAsync(channelId, message, cancellationToken: cancellationToken);
}
