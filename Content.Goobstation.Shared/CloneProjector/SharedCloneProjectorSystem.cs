// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.CloneProjector.Clone;
using Content.Shared._DV.Carrying;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.CloneProjector;

public abstract class SharedCloneProjectorSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HolographicCloneComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<HolographicCloneComponent> clone, ref MeleeHitEvent args)
    {
        if (!args.IsHit
            || clone.Comp.HostEntity is not { } host)
            return;

        // Stop clones from punching their host.
        // Don't be a shitter.
        foreach (var hitEntity in args.HitEntities)
        {
            if (hitEntity != host)
                continue;

            args.BonusDamage = -args.BaseDamage;
        }
    }


}
