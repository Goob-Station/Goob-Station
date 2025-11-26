// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;
using Content.Shared.Gibbing.Events;
using Content.Shared.Actions;
using Content.Shared.Body.Systems;

namespace Content.Goobstation.Shared.Shadowling.Systems.Abilities.Ascension;

/// <summary>
/// This handles the Annihilate abiltiy logic.
/// Gib from afar!
/// </summary>
public sealed class ShadowlingAnnihilateSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingAnnihilateComponent, AnnihilateEvent>(OnAnnihilate);
        SubscribeLocalEvent<ShadowlingAnnihilateComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingAnnihilateComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingAnnihilateComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingAnnihilateComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnAnnihilate(EntityUid uid, ShadowlingAnnihilateComponent component, AnnihilateEvent args)
    {
        if (args.Handled)
            return;

        // The gibbening
        var target = args.Target;
        if (HasComp<ShadowlingComponent>(target))
            return;

        _body.GibBody(target, contents: GibContentsOption.Gib);
        args.Handled = true;
    }
}
