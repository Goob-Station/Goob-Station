using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Weapons;

/// <summary>
/// This handles Prevention of usage of guns.
/// </summary>
public sealed class PreventGunUseSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PreventGunUseComponent,ShotAttemptedEvent>(OnGunUse);
    }

    private void OnGunUse(Entity<PreventGunUseComponent> ent,ref ShotAttemptedEvent arg)
    {
        arg.Cancel();

        if (_timing.CurTime < ent.Comp.LastPopup + ent.Comp.PopupCooldown)
            return;

        _popup.PopupEntity(Loc.GetString("interaction-misc-gun-use-prevented"),ent.Owner,ent.Owner);
        ent.Comp.LastPopup = _timing.CurTime;
    }
}
