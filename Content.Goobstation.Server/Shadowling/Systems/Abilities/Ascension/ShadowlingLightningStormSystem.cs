// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;
using Content.Server.Actions;
using Content.Server.DoAfter;
using Content.Server.Lightning;
using Content.Shared.DoAfter;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.Ascension;

/// <summary>
/// This handles the Lightning Storm ability.
/// Lightning Storm creates a lightning ball that electrocutes everyone near a specific radius
/// </summary>
public sealed class ShadowlingLightningStormSystem : EntitySystem
{
    [Dependency] private readonly LightningSystem _lightningSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingLightningStormComponent, LightningStormEvent>(OnLightningStorm);
        SubscribeLocalEvent<ShadowlingLightningStormComponent, LightningStormEventDoAfterEvent>(OnLightningStormDoAfter);
        SubscribeLocalEvent<ShadowlingLightningStormComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ShadowlingLightningStormComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingLightningStormComponent> ent, ref ComponentStartup args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingLightningStormComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnLightningStorm(EntityUid uid, ShadowlingLightningStormComponent component, LightningStormEvent args)
    {
        // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAHHHHHHHHHHHHHHHHHHHHHH
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
    }

    private void OnLightningStormDoAfter(EntityUid uid, ShadowlingLightningStormComponent component, LightningStormEventDoAfterEvent args)
    {
        if (args.Cancelled
            || args.Handled)
            return;

        args.Handled = true;
        _lightningSystem.ShootRandomLightnings(uid, component.Range, component.BoltCount, component.LightningProto);
    }
}
