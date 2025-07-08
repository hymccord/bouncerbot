using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MonstroBot;
public enum LogChannel
{
    General,
    Achievement,
    [Display(Name = "Egg Master")]
    EggMaster,
    Verification,
}
