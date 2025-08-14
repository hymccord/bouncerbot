using System.ComponentModel.DataAnnotations;

using NetCord.Services.ApplicationCommands;

namespace BouncerBot;

// An public-ish enum seen by Discord Administrators and Role Managers
public enum Role
{
    Verified,
    [SlashCommandChoice(Name = "⭐")]
    [Display(Name = "⭐")]
    Star,
    [SlashCommandChoice(Name = "👑")]
    [Display(Name = "👑")]
    Crown,
    [SlashCommandChoice(Name = "✅")]
    [Display(Name = "✅")]
    Checkmark,
    [SlashCommandChoice(Name = "🥚")]
    [Display(Name = "🥚")]
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

public enum AchievementRole : uint
{
    // Regular Achievements start at 1
    [SlashCommandChoice(Name = "⭐")]
    [Display(Name = "⭐")]
    Star = 1,
    
    [SlashCommandChoice(Name = "👑")]
    [Display(Name = "👑")]
    Crown = 2,
    
    [SlashCommandChoice(Name = "✅")]
    [Display(Name = "✅")]
    Checkmark = 3,
    
    [SlashCommandChoice(Name = "🥚")]
    [Display(Name = "🥚")]
    EggMaster = 4,

    // Mastery Achievements: start at 11 to leave room for future regular achievements
    [SlashCommandChoice(Name = "Arcane Master")]
    [Display(Name = "Arcane Master")]
    ArcaneMaster = 11,
    
    [SlashCommandChoice(Name = "Draconic Master")]
    [Display(Name = "Draconic Master")]
    DraconicMaster = 12,
    
    [SlashCommandChoice(Name = "Forgotten Master")]
    [Display(Name = "Forgotten Master")]
    ForgottenMaster = 13,
    
    [SlashCommandChoice(Name = "Hydro Master")]
    [Display(Name = "Hydro Master")]
    HydroMaster = 14,
    
    [SlashCommandChoice(Name = "Law Master")]
    [Display(Name = "Law Master")]
    LawMaster = 15,
    
    [SlashCommandChoice(Name = "Physical Master")]
    [Display(Name = "Physical Master")]
    PhysicalMaster = 16,
    
    [SlashCommandChoice(Name = "Rift Master")]
    [Display(Name = "Rift Master")]
    RiftMaster = 17,
    
    [SlashCommandChoice(Name = "Shadow Master")]
    [Display(Name = "Shadow Master")]
    ShadowMaster = 18,
    
    [SlashCommandChoice(Name = "Tactical Master")]
    [Display(Name = "Tactical Master")]
    TacticalMaster = 19,
    
    [SlashCommandChoice(Name = "Multi Master")]
    [Display(Name = "Multi Master")]
    MultiMaster = 20,
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
