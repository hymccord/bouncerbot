using Humanizer;

namespace BouncerBot;
internal class RoleNotConfiguredException : Exception
{
    public RoleNotConfiguredException(Role role)
        : base($"The Discord role for `{role.Humanize()}` has not been configured. An admin should use `/config role`.")
    {
    }
}
