namespace BouncerBot.Modules.Achieve;

/// <summary>
/// Represents the progress toward completing an achievement.
/// </summary>
public abstract record AchievementProgress
{
    /// <summary>
    /// Indicates whether the achievement is complete.
    /// </summary>
    public required bool IsComplete { get; init; }
}

/// <summary>
/// Progress toward the Crown achievement (10+ catches on all mice).
/// </summary>
public record CrownProgress : AchievementProgress
{
    /// <summary>
    /// Number of mice with 10 or more catches.
    /// </summary>
    public required uint MiceWithCrown { get; init; }

    /// <summary>
    /// Total number of mice required for the achievement.
    /// </summary>
    public required uint TotalMice { get; init; }

    /// <summary>
    /// List of mice that still need more catches (mouseName -> current catches).
    /// </summary>
    public required Dictionary<string, uint> MissingMice { get; init; }
}

/// <summary>
/// Progress toward the Star achievement (3 stars in all locations).
/// </summary>
public record StarProgress : AchievementProgress
{
    /// <summary>
    /// Number of locations with all mice caught.
    /// </summary>
    public required uint CompletedLocations { get; init; }

    /// <summary>
    /// Total number of locations.
    /// </summary>
    public required uint TotalLocations { get; init; }

    /// <summary>
    /// List of incomplete location names.
    /// </summary>
    public required List<string> IncompleteLocations { get; init; }
}

/// <summary>
/// Progress toward the Checkmark achievement (all profile categories complete).
/// </summary>
public record CheckmarkProgress : AchievementProgress
{
    /// <summary>
    /// Number of completed categories (Weapons, Bases, Maps, Collectibles, Skins).
    /// </summary>
    public required uint CompletedCategories { get; init; }

    /// <summary>
    /// Total number of categories (typically 5).
    /// </summary>
    public required uint TotalCategories { get; init; }

    /// <summary>
    /// List of incomplete category names.
    /// </summary>
    public required List<string> IncompleteCategories { get; init; }
}

/// <summary>
/// Progress toward a Power Type Master achievement (100+ catches on all mice of a specific power type).
/// </summary>
public record PowerTypeMasterProgress : AchievementProgress
{
    /// <summary>
    /// The power type being tracked.
    /// </summary>
    public required PowerType PowerType { get; init; }

    /// <summary>
    /// Number of mice of this type with 100+ catches.
    /// </summary>
    public required uint MiceWithMastery { get; init; }

    /// <summary>
    /// Total number of mice of this type.
    /// </summary>
    public required uint TotalMice { get; init; }

    /// <summary>
    /// List of mice that still need more catches (mouseName -> current catches).
    /// Sorted by closest to completion.
    /// </summary>
    public required Dictionary<string, uint> MissingMice { get; init; }
}

/// <summary>
/// Progress toward the Fabled rank achievement.
/// </summary>
public record FabledProgress : AchievementProgress
{
    /// <summary>
    /// The user's current rank.
    /// </summary>
    public required Rank CurrentRank { get; init; }
}

/// <summary>
/// Progress toward the Egg Master achievement (collect all SEH eggs).
/// </summary>
public record EggMasterProgress : AchievementProgress
{
    // Note: The MouseHunt API only returns a boolean for this achievement,
    // so no granular progress data is available.
}
