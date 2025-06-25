// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Megafauna.Components;
using Content.Server._Lavaland.Weapons;
using Content.Shared.Mobs;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._Lavaland.Megafauna.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class MegafaunaDropSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<MegafaunaDropComponent, AttackedEvent>(OnDropAttacked);
        SubscribeLocalEvent<MegafaunaDropComponent, MobStateChangedEvent>(OnDropKilled);
    }

    private void OnDropAttacked(EntityUid uid, MegafaunaDropComponent comp, ref AttackedEvent args)
    {
        if (!HasComp<MegafaunaWeaponLooterComponent>(args.Used))
            comp.CrusherOnly = false; // it's over...
    }

    private void OnDropKilled(EntityUid uid, MegafaunaDropComponent comp, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var coords = Transform(uid).Coordinates;

        RaiseLocalEvent(uid, new MegafaunaKilledEvent());

        if (comp.CrusherOnly && comp.CrusherLoot != null)
            Spawn(comp.CrusherLoot, coords);
        else if (comp.Loot != null)
            Spawn(comp.Loot, coords);

        if (comp.DeleteOnDrop)
            QueueDel(uid);
    }
}
