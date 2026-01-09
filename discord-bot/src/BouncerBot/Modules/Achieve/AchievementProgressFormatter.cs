using Humanizer;

namespace BouncerBot.Modules.Achieve;

/// <summary>
/// Provides formatting utilities for achievement progress information.
/// </summary>
public static class AchievementProgressFormatter
{
    /// <summary>
    /// Formats achievement progress into a human-readable string with bold markdown.
    /// </summary>
    /// <param name="progress">The achievement progress to format.</param>
    /// <returns>A formatted string describing the progress, or empty if complete.</returns>
    public static string GetProgressText(AchievementProgress? progress)
    {
        if (progress?.IsComplete ?? true)
        {
            return string.Empty;
        }

        return progress switch
        {
            CrownProgress crown => $"""
                -# **Progress:** {crown.MiceWithCrown}/{crown.TotalMice}
                -# **Remaining:** {crown.MissingMice.Count}
                -# **Closest:** {string.Join(", ", crown.MissingMice.Take(5).Select(m => $"{m.Key} ({m.Value}/10)"))}"
                """,

            StarProgress star => $"""
                -# **Progress:** {star.CompletedLocations}/{star.TotalLocations}
                -# **Incomplete:** {string.Join(", ", star.IncompleteLocations)}
                """,

            CheckmarkProgress checkmark => $"""
                -# **Progress:** {checkmark.CompletedCategories}/{checkmark.TotalCategories}
                -# **Incomplete:** {string.Join(", ", checkmark.IncompleteCategories)}
                """,

            PowerTypeMasterProgress master => $"""
                -# **Progress:** {master.MiceWithMastery}/{master.TotalMice}
                -# **Mice Remaining:** {master.MissingMice.Count}
                -# **Closest:** {string.Join(", ", master.MissingMice.Take(5).Select(m => $"{m.Key} ({m.Value}/100)"))}
                """,

            FabledProgress fabled => $"""
                -# **Current Rank:** {fabled.CurrentRank.Humanize()}
                """,

            EggMasterProgress => string.Empty,

            _ => string.Empty
        };
    }
}
