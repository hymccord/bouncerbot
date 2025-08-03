using System.ComponentModel.DataAnnotations;

using NetCord.Services.ApplicationCommands;

namespace BouncerBot;

// An public-ish enum seen by Discord Administrators and Role Managers
public enum Role
{
    Verified,
    [SlashCommandChoice(Name = "⭐")]
    Star,
    [SlashCommandChoice(Name = "👑")]
    Crown,
    [SlashCommandChoice(Name = "✅")]
    Checkmark,
    [SlashCommandChoice(Name = "🥚")]
    [Display(Name = "Egg Master")]
    EggMaster,
    [SlashCommandChoice(Name = "Achiever 🍪")]
    Achiever,
    [SlashCommandChoice(Name = "Arcane Master")]
    [Display(Name = "Arcane Master")]
    ArcaneMaster,
    [SlashCommandChoice(Name = "Draconic Master")]
    [Display(Name = "Draconic Master")]
    DraconicMaster,
    [SlashCommandChoice(Name = "Forgotten Master")]
    [Display(Name = "Forgotten Master")]
    ForgottenMaster,
    [SlashCommandChoice(Name = "Hydro Master")]
    [Display(Name = "Hydro Master")]
    HydroMaster,
    [SlashCommandChoice(Name = "Law Master")]
    [Display(Name = "Law Master")]
    LawMaster,
    [SlashCommandChoice(Name = "Physical Master")]
    [Display(Name = "Physical Master")]
    PhysicalMaster,
    [SlashCommandChoice(Name = "Rift Master")]
    [Display(Name = "Rift Master")]
    RiftMaster,
    [SlashCommandChoice(Name = "Shadow Master")]
    [Display(Name = "Shadow Master")]
    ShadowMaster,
    [SlashCommandChoice(Name = "Tactical Master")]
    [Display(Name = "Tactical Master")]
    TacticalMaster,
    [SlashCommandChoice(Name = "Multi Master")]
    [Display(Name = "Multi Master")]
    MultiMaster,
}

// Public facing enum for Discord command parameters
public enum AchievementRole
{
    [SlashCommandChoice(Name = "⭐")]
    Star,
    [SlashCommandChoice(Name = "👑")]
    Crown,
    [SlashCommandChoice(Name = "✅")]
    Checkmark,
    [SlashCommandChoice(Name = "🥚")]
    [Display(Name = "Egg Master")]
    EggMaster,
    [SlashCommandChoice(Name = "Arcane Master")]
    [Display(Name = "Arcane Master")]
    ArcaneMaster,
    [SlashCommandChoice(Name = "Draconic Master")]
    [Display(Name = "Draconic Master")]
    DraconicMaster,
    [SlashCommandChoice(Name = "Forgotten Master")]
    [Display(Name = "Forgotten Master")]
    ForgottenMaster,
    [SlashCommandChoice(Name = "Hydro Master")]
    [Display(Name = "Hydro Master")]
    HydroMaster,
    [SlashCommandChoice(Name = "Law Master")]
    [Display(Name = "Law Master")]
    LawMaster,
    [SlashCommandChoice(Name = "Physical Master")]
    [Display(Name = "Physical Master")]
    PhysicalMaster,
    [SlashCommandChoice(Name = "Rift Master")]
    [Display(Name = "Rift Master")]
    RiftMaster,
    [SlashCommandChoice(Name = "Shadow Master")]
    [Display(Name = "Shadow Master")]
    ShadowMaster,
    [SlashCommandChoice(Name = "Tactical Master")]
    [Display(Name = "Tactical Master")]
    TacticalMaster,
    [SlashCommandChoice(Name = "Multi Master")]
    [Display(Name = "Multi Master")]
    MultiMaster,
}

public enum PowerType
{
    None, // Special case for event mice
    Arcane,
    Draconic,
    Forgotten,
    Hydro,
    Law,
    Physical,
    Rift,
    Shadow,
    Tactical,
    Multi
}

public enum Rank
{
    Novice,
    Recruit,
    Apprentice,
    Initiate,
    Journeyman,
    Master,
    Grandmaster,
    Legendary,
    Hero,
    Knight,
    Lord,
    Baron,
    Count,
    Duke,
    GrandDuke,
    Archduke,
    Viceroy,
    Elder,
    Sage,
    Fabled,
}

public static class EnumUtils
{
    public static Role ToRole(AchievementRole achievementRole)
    {
        return achievementRole switch
        {
            AchievementRole.Star => Role.Star,
            AchievementRole.Crown => Role.Crown,
            AchievementRole.Checkmark => Role.Checkmark,
            AchievementRole.EggMaster => Role.EggMaster,
            AchievementRole.ArcaneMaster => Role.ArcaneMaster,
            AchievementRole.DraconicMaster => Role.DraconicMaster,
            AchievementRole.ForgottenMaster => Role.ForgottenMaster,
            AchievementRole.HydroMaster => Role.HydroMaster,
            AchievementRole.LawMaster => Role.LawMaster,
            AchievementRole.PhysicalMaster => Role.PhysicalMaster,
            AchievementRole.RiftMaster => Role.RiftMaster,
            AchievementRole.ShadowMaster => Role.ShadowMaster,
            AchievementRole.TacticalMaster => Role.TacticalMaster,
            AchievementRole.MultiMaster => Role.MultiMaster,
            _ => throw new ArgumentOutOfRangeException(nameof(achievementRole), achievementRole, null),
        };
    }

    public static AchievementRole ToAchievementRole(Role role)
    {
        return role switch
        {
            Role.Star => AchievementRole.Star,
            Role.Crown => AchievementRole.Crown,
            Role.Checkmark => AchievementRole.Checkmark,
            Role.EggMaster => AchievementRole.EggMaster,
            Role.ArcaneMaster => AchievementRole.ArcaneMaster,
            Role.DraconicMaster => AchievementRole.DraconicMaster,
            Role.ForgottenMaster => AchievementRole.ForgottenMaster,
            Role.HydroMaster => AchievementRole.HydroMaster,
            Role.LawMaster => AchievementRole.LawMaster,
            Role.PhysicalMaster => AchievementRole.PhysicalMaster,
            Role.RiftMaster => AchievementRole.RiftMaster,
            Role.ShadowMaster => AchievementRole.ShadowMaster,
            Role.TacticalMaster => AchievementRole.TacticalMaster,
            Role.MultiMaster => AchievementRole.MultiMaster,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null),
        };
    }
}
