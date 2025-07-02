namespace MonstroBot.Db.Models;
public class AchievementMessage
{
    public ulong GuildId { get; set; }

    public string? Star { get; set; }
    public string? Crown { get; set; }
    public string? Checkmark { get; set; }
    public string? EggMaster { get; set; }

    public string? ArcaneMaster { get; set; }
    public string? DraconicMaster { get; set; }
    public string? ForgottenMaster { get; set; }
    public string? HyderoMaster { get; set; }
    public string? LawMaster { get; set; }
    public string? PhysicalMaster { get; set; }
    public string? RiftMaster { get; set; }
    public string? ShadowMaster { get; set; }
    public string? TacticalMaster { get; set; }
    public string? MultiMaster { get; set; }
}
