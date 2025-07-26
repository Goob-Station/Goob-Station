// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;
using Content.Server.Actions;
using Content.Shared.Gibbing.Events;
using Content.Server.Body.Systems;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.Ascension;

/// <summary>
/// This handles the Annihilate abiltiy logic.
/// Gib from afar!
/// </summary>
public sealed class ShadowlingAnnihilateSystem : EntitySystem
{
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingAnnihilateComponent, AnnihilateEvent>(OnAnnihilate);
    }

    private void OnAnnihilate(EntityUid uid, ShadowlingAnnihilateComponent component, AnnihilateEvent args)
    {
        // The gibbening
        var target = args.Target;
        if (HasComp<ShadowlingComponent>(target))
            return;

        _body.GibBody(target, contents: GibContentsOption.Gib);

        _actions.StartUseDelay(args.Action);
    }
}
