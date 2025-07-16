using System.ComponentModel.DataAnnotations;

using NetCord.Services.ApplicationCommands;

namespace BouncerBot;

public enum Role
{
    Verified,
    [SlashCommandChoice(Name = "Map Banned")]
    [Display(Name = "Map Banned")]
    MapBanned,
    [SlashCommandChoice(Name = "Trade Banned")]
    [Display(Name = "Trade Banned")]
    TradeBanned,
    Star,
    Crown,
    Checkmark,
    Achiever,
    [SlashCommandChoice(Name = "Egg Master")]
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

public enum AchievementRole
{
    Star,
    Crown,
    Checkmark,
    [SlashCommandChoice(Name = "Egg Master")]
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
