// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;
using Content.Server.Actions;
using Content.Shared.Humanoid;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.Ascension;

/// <summary>
/// This handles Hypnosis.
/// Instant Thrall from afar!
/// </summary>
public sealed class ShadowlingHypnosisSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingHypnosisComponent, HypnosisEvent>(OnHypnosis);
        SubscribeLocalEvent<ShadowlingHypnosisComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ShadowlingHypnosisComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingHypnosisComponent> ent, ref ComponentStartup args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingHypnosisComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnHypnosis(EntityUid uid, ShadowlingHypnosisComponent component, HypnosisEvent args)
    {
        var target = args.Target;
        if (HasComp<ThrallComponent>(target) || HasComp<ShadowlingComponent>(target))
            return;

        if (!HasComp<HumanoidAppearanceComponent>(target))
            return;

        EnsureComp<ThrallComponent>(target);
        _actions.StartUseDelay((args.Action.Owner, args.Action.Comp));
    }
}
