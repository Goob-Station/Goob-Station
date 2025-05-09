// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goidastation.Common.Standing;
using Content.Server.Item;
using Content.Shared._EinsteinEngines.TelescopicBaton;
using Content.Shared._White.Standing;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.GameObjects;

namespace Content.Server._EinsteinEngines.TelescopicBaton;

public sealed class TelescopicBatonSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!; // Goidastation
    [Dependency] private readonly ItemSystem _item = default!; // Goidastation
    [Dependency] private readonly UseDelaySystem _delay = default!; // Goidastation

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TelescopicBatonComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TelescopicBatonComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<TelescopicBatonComponent, KnockdownOnHitAttemptEvent>(OnKnockdownAttempt);
        SubscribeLocalEvent<TelescopicBatonComponent, MeleeHitEvent>(OnMeleeHit, after: new[] { typeof(KnockdownOnHitSystem) }); // Goidastation
    }

    private void OnMeleeHit(Entity<TelescopicBatonComponent> ent, ref MeleeHitEvent args) // Goidastation
    {
        if (!ent.Comp.AlwaysDropItems)
            ent.Comp.CanDropItems = false; // Goida edit

        if (args is { IsHit: true, HitEntities.Count: > 0 } && TryComp(ent, out UseDelayComponent? delay))
            _delay.ResetAllDelays((ent, delay));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TelescopicBatonComponent>();
        while (query.MoveNext(out var baton))
        {
            if (baton.AlwaysDropItems) // Goidastation
                continue;

            if (!baton.CanDropItems) // Goida edit
                continue;

            baton.TimeframeAccumulator += TimeSpan.FromSeconds(frameTime);
            if (baton.TimeframeAccumulator <= baton.AttackTimeframe)
                continue;

            baton.CanDropItems = false; // Goida edit
            baton.TimeframeAccumulator = TimeSpan.Zero;
        }
    }

    private void OnMapInit(Entity<TelescopicBatonComponent> baton, ref MapInitEvent args)
    {
        ToggleBaton(baton, false);
    }

    private void OnToggled(Entity<TelescopicBatonComponent> baton, ref ItemToggledEvent args)
    {
        _item.SetHeldPrefix(baton, args.Activated ? "on" : "off"); // Goidastation
        ToggleBaton(baton, args.Activated);
    }

    private void OnKnockdownAttempt(Entity<TelescopicBatonComponent> baton, ref KnockdownOnHitAttemptEvent args)
    {
        // Goida edit start
        if (!_toggle.IsActivated(baton.Owner))
        {
            args.Cancelled = true;
            return;
        }

        if (!baton.Comp.CanDropItems)
            args.Behavior = DropHeldItemsBehavior.NoDrop;
        // Goida edit end
    }

    public void ToggleBaton(Entity<TelescopicBatonComponent> baton, bool state)
    {
        baton.Comp.TimeframeAccumulator = TimeSpan.Zero;
        baton.Comp.CanDropItems = state; // Goida edit
        _appearance.SetData(baton, TelescopicBatonVisuals.State, state);
    }
}