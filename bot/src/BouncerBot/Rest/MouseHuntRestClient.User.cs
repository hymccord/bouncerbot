using System.Reflection;
using System.Text.Json;

using BouncerBot.Db.Models;
using BouncerBot.Rest.Models;

using Microsoft.EntityFrameworkCore;

namespace BouncerBot.Rest;
partial class MouseHuntRestClient
{
    public async Task<User> GetMeAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendRequestAsync<User>(HttpMethod.Post, "get/user/me", cancellationToken);

        return response;
    }

    public async Task<bool> IsEggMaster(uint mhId, CancellationToken cancellationToken = default)
    {
        var snuid = await GetUserSnuIdAsync(mhId, cancellationToken);

        var formData = new List<KeyValuePair<string, string>>()
        {
            new ("sn_user_ids[]", snuid.SnUserId),
            new ("fields[]", "is_egg_master"),
        };

        var document = await SendDesktopRequestAsync<JsonElement>(HttpMethod.Post, formData, "managers/ajax/users/userData.php", cancellationToken);

        var json = document.GetRawText();
        var result = document
            .GetProperty(snuid.SnUserId);

        throw new NotImplementedException();
    }

    public async Task<UserItemCategoryCompletion> GetUserLocationStatsAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var snuid = await GetUserSnuIdAsync(mhId, cancellationToken);

        var formData = new List<KeyValuePair<string, string>>()
        {
            new ("page_class", "HunterProfile"),
            new ("page_arguments[legacyMode]", ""),
            new ("page_arguments[tab]", "mice"),
            new ("page_arguments[sub_tab]", "location"),
            new ("page_arguments[snuid]", snuid.SnUserId),
        };

        var document = await SendDesktopRequestAsync<JsonElement>(HttpMethod.Post, formData, "managers/ajax/pages/page.php", cancellationToken);

        var result = document
            .GetProperty("page")
            .GetProperty("tabs")
            .GetProperty("mice")
            .GetProperty("subtabs")[1]
            .GetProperty("mouse_list")
            .Deserialize<UserItemCategoryCompletion>(_jsonSerializerOptions) ?? new UserItemCategoryCompletion();

        return result;
    }

    public async Task<UserMouseStatistics> GetUserMiceAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        var snuid = await GetUserSnuIdAsync(mhId, cancellationToken);

        return await SendRequestAsync<UserMouseStatistics>(HttpMethod.Post, $"get/user/{snuid.SnUserId}/mice", cancellationToken);
    }

    public async Task<UserItemCategoryCompletion> GetUserProfileItems(uint mhId, CancellationToken cancellationToken = default)
    {
        var snuid = await GetUserSnuIdAsync(mhId, cancellationToken);

        var formData = new List<KeyValuePair<string, string>>()
        {
            new ("page_class", "HunterProfile"),
            new ("page_arguments[legacyMode]", ""),
            new ("page_arguments[tab]", "items"),
            new ("page_arguments[sub_tab]", "false"),
            new ("page_arguments[snuid]", snuid.SnUserId),
        };

        var document = await SendDesktopRequestAsync<JsonElement>(HttpMethod.Post, formData, "managers/ajax/pages/page.php", cancellationToken);

        var result = document
            .GetProperty("page")
            .GetProperty("tabs")
            .GetProperty("items")
            .GetProperty("subtabs")[0]
            .GetProperty("items")
            .Deserialize<UserItemCategoryCompletion>(_jsonSerializerOptions) ?? new UserItemCategoryCompletion();

        return result;
    }

    public async Task<UserSnuIdInfo> GetUserSnuIdAsync(uint mhId, CancellationToken cancellationToken = default)
    {
        // Check cache first
        using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var cached = await context.SnuidCache.SingleOrDefaultAsync(s => s.MouseHuntId == mhId, cancellationToken);

        if (cached is not null)
        {
            return new UserSnuIdInfo { SnUserId = cached.SnuId };
        }

        // Fetch from API
        var result = await SendRequestAsync<UserSnuIdInfo>(HttpMethod.Post, $"get/usersnuid/{mhId}", cancellationToken);
        
        // Cache the result
        context.SnuidCache.Add(new Snuid { MouseHuntId = mhId, SnuId = result.SnUserId });
        await context.SaveChangesAsync(cancellationToken);
        
        return result;
    }
}
