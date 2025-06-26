// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.CloneProjector.Clone;
using Content.Shared.Nyanotrasen.Holograms;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Goobstation.Shared.CloneProjector;

public abstract class SharedCloneProjectorSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HolographicCloneComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HolographicCloneComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<HolographicCloneComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnStartup(Entity<HolographicCloneComponent> clone, ref ComponentStartup args)
    {
        EnsureComp<HologramVisualsComponent>(clone);
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

    private void OnShotAttempted(Entity<HolographicCloneComponent> ent, ref ShotAttemptedEvent args)
    {
        if (ent.Comp.HostProjector is not { } hostProjector
            || !hostProjector.Comp.RestrictRangedWeapons)
            return;

        _popupSystem.PopupClient(Loc.GetString("gun-disabled"), ent, ent);
        args.Cancel();
    }

}
