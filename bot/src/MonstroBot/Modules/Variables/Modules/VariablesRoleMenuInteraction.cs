using Humanizer;

using MonstroBot.Attributes;
using MonstroBot.Db;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace MonstroBot.Modules.Variables.Modules;

[GuildOnly<RoleMenuInteractionContext>]
public class VariablesRoleMenuInteraction(MonstroBotDbContext dbContext) : ComponentInteractionModule<RoleMenuInteractionContext>
{
    [ComponentInteraction("variables roles menu")]
    public async Task CreateRoleMenuAsync(Role role)
    {
        await RespondAsync(InteractionCallback.DeferredModifyMessage);

        var guildId = Context.Guild!.Id;
        var selectedRole = Context.SelectedRoles[0].Id;

        var roleSetting = dbContext.RoleSettings.Find(guildId);

        if (roleSetting is null)
        {
            roleSetting = new Db.Models.RoleSetting
            {
                GuildId = guildId
            };

            dbContext.RoleSettings.Add(roleSetting);
        }

        switch (role)
        {
            case Role.Verified:
                roleSetting.VerifiedId = selectedRole;
                break;
            case Role.MapBanned:
                roleSetting.MapBannedId = selectedRole;
                break;
            case Role.TradeBanned:
                roleSetting.TradeBannedId = selectedRole;
                break;
            case Role.Star:
                roleSetting.StarId = selectedRole;
                break;
            case Role.Crown:
                roleSetting.CrownId = selectedRole;
                break;
            case Role.Checkmark:
                roleSetting.CheckmarkId = selectedRole;
                break;
            case Role.EggMaster:
                roleSetting.EggMasterId = selectedRole;
                break;
            case Role.ArcaneMaster:
                roleSetting.ArcaneMasterId = selectedRole;
                break;
            case Role.DraconicMaster:
                roleSetting.DraconicMasterId = selectedRole;
                break;
            case Role.ForgottenMaster:
                roleSetting.ForgottenMasterId = selectedRole;
                break;
            case Role.HydroMaster:
                roleSetting.HydroMasterId = selectedRole;
                break;
            case Role.LawMaster:
                roleSetting.LawMasterId = selectedRole;
                break;
            case Role.PhysicalMaster:
                roleSetting.PhysicalMasterId = selectedRole;
                break;
            case Role.RiftMaster:
                roleSetting.RiftMasterId = selectedRole;
                break;
            case Role.ShadowMaster:
                roleSetting.ShadowMasterId = selectedRole;
                break;
            case Role.TacticalMaster:
                roleSetting.TacticalMasterId = selectedRole;
                break;
            case Role.MultiMaster:
                roleSetting.MultiMasterId = selectedRole;
                break;
            default:
                break;
        }

        await dbContext.SaveChangesAsync();
        await ModifyResponseAsync(m =>
        {
            m.Content = $"Set {role.Humanize(LetterCasing.Title)} role to <@&{selectedRole}>.";
            m.Flags = MessageFlags.Ephemeral;
            m.Components = [];
            m.Embeds = [];
            m.AllowedMentions = AllowedMentionsProperties.None;
        });
        await Task.Delay(3000);
        await DeleteResponseAsync();
    }
}
