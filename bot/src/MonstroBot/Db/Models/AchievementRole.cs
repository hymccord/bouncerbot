namespace MonstroBot.Db.Models;
public class AchievementRole
{
    public ulong GuildId { get; set; }

    public ulong? StarId { get; set; }
    public ulong? CrownId { get; set; }
    public ulong? CheckmarkId { get; set; }
    public ulong? EggMasterId { get; set; }

    public ulong? ArcaneMasterId { get; set; }
    public ulong? DraconicMasterId { get; set; }
    public ulong? ForgottenMasterId { get; set; }
    public ulong? HyderoMasterId { get; set; }
    public ulong? LawMasterId { get; set; }
    public ulong? PhysicalMasterId { get; set; }
    public ulong? RiftMasterId { get; set; }
    public ulong? ShadowMasterId { get; set; }
    public ulong? TacticalMasterId { get; set; }
    public ulong? MultiMasterId { get; set; }
}
