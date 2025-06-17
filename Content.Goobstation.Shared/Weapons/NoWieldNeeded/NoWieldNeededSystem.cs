// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
        SubscribeLocalEvent<NoWieldNeededComponent, ComponentShutdown>(OnComponentShutdown);
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

        comp.GunsWithBonus.Add(args.Entity);
    }

    private void OnGunDropped(EntityUid uid, NoWieldNeededComponent comp, EntRemovedFromContainerMessage args)
    {
        if (!comp.GetBonus)
            return;

        comp.GunsWithBonus.Remove(args.Entity);
        RevertGun(args.Entity);
    }

    private void OnComponentShutdown(Entity<NoWieldNeededComponent> ent, ref ComponentShutdown args)
    {
        ent.Comp.GunsWithBonus.ForEach(RevertGun);
    }

    private void RevertGun(EntityUid uid)
    {
        if (!TryComp<GunComponent>(uid, out var gun) || !TryComp<GunWieldBonusComponent>(uid, out var bonus))
            return;

        gun.MinAngle -= bonus.MinAngle;
        gun.MaxAngle -= bonus.MaxAngle;
        gun.AngleDecay -= bonus.AngleDecay;
        gun.AngleIncrease -= bonus.AngleIncrease;
        _gun.RefreshModifiers(uid);
    }
}
