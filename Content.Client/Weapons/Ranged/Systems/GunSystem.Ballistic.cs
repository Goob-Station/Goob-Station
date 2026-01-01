// SPDX-FileCopyrightText: 2022 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 T-Stalker <43253663+DogZeroX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 T-Stalker <le0nel_1van@hotmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Map;

namespace Content.Client.Weapons.Ranged.Systems;

public sealed partial class GunSystem
{
    protected override void InitializeBallistic()
    {
        base.InitializeBallistic();
        SubscribeLocalEvent<BallisticAmmoProviderComponent, UpdateAmmoCounterEvent>(OnBallisticAmmoCount);
    }

    private void OnBallisticAmmoCount(Entity<BallisticAmmoProviderComponent> ent, ref UpdateAmmoCounterEvent args)
    {
        if (args.Control is DefaultStatusControl control)
        {
            control.Update(GetBallisticShots(ent.Comp), ent.Comp.Capacity);
        }
    }

    protected override void Cycle(Entity<BallisticAmmoProviderComponent> ent, MapCoordinates coordinates)
    {
        if (!Timing.IsFirstTimePredicted)
            return;

        EntityUid? ammoEnt = null;

        // TODO: Combine with TakeAmmo
        if (ent.Comp.Entities.Count > 0)
        {
            var existing = ent.Comp.Entities[^1];
            ent.Comp.Entities.RemoveAt(ent.Comp.Entities.Count - 1);

            Containers.Remove(existing, ent.Comp.Container);
            EnsureShootable(existing);
        }
        else if (ent.Comp.UnspawnedCount > 0)
        {
            ent.Comp.UnspawnedCount--;
            ammoEnt = Spawn(ent.Comp.Proto, coordinates);
            EnsureShootable(ammoEnt.Value);
        }

        if (ammoEnt != null && IsClientSide(ammoEnt.Value))
            Del(ammoEnt.Value);

        var cycledEvent = new GunCycledEvent();
        RaiseLocalEvent(ent, ref cycledEvent);
    }
}