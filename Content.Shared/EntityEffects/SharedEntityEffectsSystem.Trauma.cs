using Content.Goobstation.Common.Chemistry;

namespace Content.Shared.EntityEffects;

/// <summary>
/// Trauma - helper for allowing reactive effects to be cancelled.
/// </summary>
public sealed partial class SharedEntityEffectsSystem
{
    public bool AllowedToReact(EntityUid uid)
    {
        var ev = new BeforeSolutionReactEvent();
        RaiseLocalEvent(uid, ref ev);
        return !ev.Cancelled;
    }
}
