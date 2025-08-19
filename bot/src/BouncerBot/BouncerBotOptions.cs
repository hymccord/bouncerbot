using System.ComponentModel.DataAnnotations;

namespace BouncerBot;
public class BouncerBotOptions
{
    [Required]
    public required EmojiOptions Emojis { get; set; }

    [Required]
    public required ColorOptions Colors { get; set;}

    [Required]
    public required ulong PuzzleChannel { get; set; }

    [Required]
    public required string MouseHuntUrl { get; set; }

    public DebugOptions Debug { get; set; } = new();
}

public class EmojiOptions
{
    [Required]
    public required string Arcane { get; set; }
    [Required]
    public required string Draconic { get; set; }
    [Required]
    public required string Forgotten { get; set; }
    [Required]
    public required string Hydro { get; set; }
    [Required]
    public required string Law { get; set; }
    [Required]
    public required string Physical { get; set; }
    [Required]
    public required string Rift { get; set; }
    [Required]
    public required string Shadow { get; set; }
    [Required]
    public required string Tactical { get; set; }
    [Required]
    public required string Multi { get; set; }

    public TitleEmojis Titles { get; set; } = new TitleEmojis();

    public class TitleEmojis
    {
        public string? Viceroy { get; set; }
        public string? Archduke { get; set; }
        public string? GrandDuke { get; set; }
        public string? Duke { get; set; }
        public string? Count { get; set; }
        public string? Baron { get; set; }
        public string? Lord { get; set; }
        public string? Knight { get; set; }
        public string? Hero { get; set; }
        public string? Legendary { get; set; }
        public string? Grandmaster { get; set; }
        public string? Master { get; set; }
        public string? Journeyman { get; set; }
        public string? Initiate { get; set; }
        public string? Apprentice { get; set; }
        public string? Recruit { get; set; }
        public string? Novice { get; set; }
        public string? Fabled { get; set; }
        public string? Sage { get; set; }
        public string? Elder { get; set; }
    }
}

public class ColorOptions
{
    [Required]
    public required int Success { get; set; }

    [Required]
    public required int Error { get; set; }

    [Required]
    public required int Warning { get; set; }

    [Required]
    public required int Primary { get; set; }
}

public class DebugOptions
{
    public bool DisableAchievementCheck { get; set; } = false;
}
