using System.Diagnostics.CodeAnalysis;

namespace Content.Goobstation.Common.Morgue;

// Le experimental way to communicate between core and custom
public abstract class CommonGoobCrematoriumSystem : EntitySystem
{
    public abstract bool IsAllowed(EntityUid uid, EntityUid user);
    public abstract bool CanCremate(EntityUid uid, EntityUid target, [NotNullWhen(false)] out string? reason);
    public abstract void TryDeleteItems(EntityUid target, EntityUid crematorium);
    public abstract void LogPassedChecks(EntityUid user, EntityUid target);
}
