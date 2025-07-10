using System.ComponentModel.DataAnnotations;

namespace BouncerBot;

public enum Role
{
    Verified,
    [Display(Name = "Map Banned")]
    MapBanned,
    [Display(Name = "Trade Banned")]
    TradeBanned,
    Star,
    Crown,
    Checkmark,
    [Display(Name = "Egg Master")]
    EggMaster,
    [Display(Name = "Arcane Master")]
    ArcaneMaster,
    [Display(Name = "Draconic Master")]
    DraconicMaster,
    [Display(Name = "Forgotten Master")]
    ForgottenMaster,
    [Display(Name = "Hydro Master")]
    HydroMaster,
    [Display(Name = "Law Master")]
    LawMaster,
    [Display(Name = "Physical Master")]
    PhysicalMaster,
    [Display(Name = "Rift Master")]
    RiftMaster,
    [Display(Name = "Shadow Master")]
    ShadowMaster,
    [Display(Name = "Tactical Master")]
    TacticalMaster,
    [Display(Name = "Multi Master")]
    MultiMaster,
}
