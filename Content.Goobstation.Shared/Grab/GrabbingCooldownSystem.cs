using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Grab;

public sealed class GrabbingCooldownSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;


    public bool IsCooldownReady<T>(Entity<T> ent) where T : Component, IGrabCooldownComponent
    {
        if (ent.Comp.IsCooldownActive(_gameTiming.CurTime) &&
            _container.TryGetContainingContainer(ent.Owner, out var container))
        {
            GrabPopup(ent, container);
            return false;
        }

        ent.Comp.StartCooldown(_gameTiming.CurTime);
        return true;
    }

    private void GrabPopup<T>(Entity<T> ent, BaseContainer container) where T : Component, IGrabCooldownComponent
    {
        var holder = container.Owner;
        _popup.PopupPredictedCursor(
            $"Your {MetaData(ent).EntityName} {Loc.GetString(ent.Comp.GrabCooldownVerb)} as it's not ready yet, wait {(ent.Comp.GrabCooldownEnd - _gameTiming.CurTime).TotalSeconds:0.0}s!",
            holder, PopupType.LargeCaution);
    }
}
