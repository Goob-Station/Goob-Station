using Content.Shared.Mobs;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.Server._Lavaland.Mobs;

public sealed class MegafaunaSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MegafaunaComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<MegafaunaComponent, MobStateChangedEvent>(OnDeath);
    }

    public void OnAttacked<T>(EntityUid uid, T comp, ref AttackedEvent args) where T : MegafaunaComponent
    {
        if (!HasComp<MegafaunaWeaponLooterComponent>(args.Used))
            comp.CrusherOnly = false; // it's over...
    }

    public void OnDeath<T>(EntityUid uid, T comp, ref MobStateChangedEvent args) where T : MegafaunaComponent
    {
        var coords = Transform(uid).Coordinates;

        comp.CancelToken.Cancel();
        RaiseLocalEvent(uid, new MegafaunaShutdownEvent());

        if (comp.CrusherOnly && comp.CrusherLoot != null)
        {
            Spawn(comp.CrusherLoot, coords);
        }
        else if (comp.Loot != null)
        {
            Spawn(comp.Loot, coords);
        }

        // Bruh.
        Timer.Spawn(TimeSpan.FromSeconds(0.2), ( ) => Del(uid));
    }
}
