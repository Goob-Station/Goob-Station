// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;
using Content.Server.Lightning;
using Content.Shared.Actions;
using Content.Shared.DoAfter;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.Ascension;

/// <summary>
/// This handles the Lightning Storm ability.
/// Lightning Storm creates a lightning ball that electrocutes everyone near a specific radius
/// </summary>
public sealed class ShadowlingLightningStormSystem : EntitySystem
{
    [Dependency] private readonly LightningSystem _lightningSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingLightningStormComponent, LightningStormEvent>(OnLightningStorm);
        SubscribeLocalEvent<ShadowlingLightningStormComponent, LightningStormEventDoAfterEvent>(OnLightningStormDoAfter);
        SubscribeLocalEvent<ShadowlingLightningStormComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingLightningStormComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingLightningStormComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingLightningStormComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnLightningStorm(EntityUid uid, ShadowlingLightningStormComponent component, LightningStormEvent args)
    {
        if (args.Handled)
            return;

        // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAHHHHHHHHHHHHHHHHHHHHHH
        // calm down bro what is this scream all about....
        var doAfter = new DoAfterArgs(
            EntityManager,
            args.Performer,
            component.TimeBeforeActivation,
            new LightningStormEventDoAfterEvent(),
            uid)
        {
            BreakOnDamage = true,
            CancelDuplicate = true,
        };
        _doAfterSystem.TryStartDoAfter(doAfter);
        args.Handled = true;
    }

    private void OnLightningStormDoAfter(EntityUid uid, ShadowlingLightningStormComponent component, LightningStormEventDoAfterEvent args)
    {
        if (args.Cancelled
            || args.Handled)
            return;

        _lightningSystem.ShootRandomLightnings(uid, component.Range, component.BoltCount, component.LightningProto);
        args.Handled = true;
    }
}
