using Humanizer;

namespace BouncerBot;
internal class RoleNotConfiguredException(Role role)
    : Exception($"The Discord role for `{role.Humanize()}` has not been configured. An admin should use `/config role`.")
{
}

internal class MessageNotConfiguredException(AchievementRole achievement)
    : Exception($"The message for the `{achievement.Humanize()}` achievement has not been configured. An admin should use `/config message`.")
{
}
