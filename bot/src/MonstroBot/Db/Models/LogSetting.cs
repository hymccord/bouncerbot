using System.ComponentModel.DataAnnotations;

namespace MonstroBot.Db.Models;
public class LogSetting
{
    public ulong GuildId { get; set; }

    public ulong? VerificationId { get; set; }

    public ulong? FlexId { get; set; }

    public ulong? EggMasterId { get; set; }

    public ulong? LogId { get; set; }
}
