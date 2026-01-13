namespace BouncerBot.Rest.Models;

public class Mouse
{
    public uint MouseId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    //public string Description { get; set; } = string.Empty;

    public string Group { get; set; } = string.Empty;

    //public string Subgroup { get; set; } = string.Empty;

    //public string SubgroupName { get; set; } = string.Empty;

    //public Weaknesses Weaknesses { get; set; } = new();
    public Dictionary<string, uint> Weaknesses { get; set; } = new();
}

//public class Weaknesses
//{
//    public int Phscl { get; set; }
//    public int Shdw { get; set; }
//    public int Tctcl { get; set; }
//    public int Arcn { get; set; }
//    public int Frgttn { get; set; }
//    public int Hdr { get; set; }
//    public int Drcnc { get; set; }
//    public int Prntl { get; set; }
//    public int Law { get; set; }
//    public int Rift { get; set; }
//}
