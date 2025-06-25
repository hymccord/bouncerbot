using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonstroBot.Db.Models;
public class VerifiedUser
{
    public ulong GuildId { get; init; }
    public ulong DiscordId { get; init; }
    public uint MouseHuntId { get; init; }
}
