using System.ComponentModel.DataAnnotations;

namespace BouncerBot;
public enum LogChannel
{
    General,
    Achievement,
    [Display(Name = "Egg Master")]
    EggMaster,
    Verification,
}
