using Content.Goobstation.Common.Weapons.NoWieldNeeded;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Wieldable;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Weapons.NoWieldNeeded;

public sealed class NoWieldNeededSystem : EntitySystem
{
    [Dependency] private readonly SharedGunSystem _gun = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GunComponent, WieldAttemptEvent>(OnWieldAttemptEvent);
        SubscribeLocalEvent<NoWieldNeededComponent, EntInsertedIntoContainerMessage>(OnGunPickedUp);
        SubscribeLocalEvent<NoWieldNeededComponent, EntRemovedFromContainerMessage>(OnGunDropped);
    }

    private void OnWieldAttemptEvent(Entity<GunComponent> ent, ref WieldAttemptEvent args)
    {
        if (TryComp<NoWieldNeededComponent>(args.User, out var noWieldNeeded) && noWieldNeeded.GetBonus)
            args.Cancel(); // cancel any attempts to wield weapons if you get no bonus from it
    }
    private void OnGunPickedUp(EntityUid uid, NoWieldNeededComponent comp, EntInsertedIntoContainerMessage args)
    {
        if (!comp.GetBonus || !TryComp<GunComponent>(args.Entity, out var gun) || !TryComp<GunWieldBonusComponent>(args.Entity, out var bonus))
            return;

        gun.MinAngle += bonus.MinAngle;
        gun.MaxAngle += bonus.MaxAngle;
        gun.AngleDecay += bonus.AngleDecay;
        gun.AngleIncrease += bonus.AngleIncrease;

        _gun.RefreshModifiers(args.Entity);
    }

    private void OnGunDropped(EntityUid uid, NoWieldNeededComponent comp, EntRemovedFromContainerMessage args)
    {
        if (!comp.GetBonus || !TryComp<GunComponent>(args.Entity, out var gun) || !TryComp<GunWieldBonusComponent>(args.Entity, out var bonus))
            return;

        gun.MinAngle -= bonus.MinAngle;
        gun.MaxAngle -= bonus.MaxAngle;
        gun.AngleDecay -= bonus.AngleDecay;
        gun.AngleIncrease -= bonus.AngleIncrease;

        _gun.RefreshModifiers(args.Entity);
    }

}
