using Content.Pirate.Shared.Avali.Components;
using Content.Pirate.Shared.Avali.EntitySystems;

namespace Content.Pirate.Server.Avali.EntitySystems;

/// <summary>
/// Instantiates the shared stasis-freeze behavior on the server.
/// </summary>
public sealed class StasisFrozenSystem : SharedStasisFrozenSystem
{
    public void FreezeAndMute(EntityUid uid)
    {
        var component = EnsureComp<StasisFrozenComponent>(uid);
        component.Muted = false;
        Dirty(uid, component);
    }
}
