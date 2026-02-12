// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Systems.Abilities.CollectiveMind;

/// <summary>
/// This handles the logic of the Collective Mind ability.
/// The Collective Mind ability lets you gain new actions, and informs you
/// how many Thralls are required to ascend. At the same time, it stuns all Thralls for a very short amount of time.
/// </summary>
public sealed class ShadowlingCollectiveMindSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingCollectiveMindComponent, CollectiveMindEvent>(OnCollectiveMind);
        SubscribeLocalEvent<ShadowlingCollectiveMindComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingCollectiveMindComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingCollectiveMindComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingCollectiveMindComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnCollectiveMind(EntityUid uid, ShadowlingCollectiveMindComponent comp, CollectiveMindEvent args)
    {
        if (args.Handled
            || !TryComp<ShadowlingComponent>(uid, out var sling))
            return;

        if (comp.UnlockedAbilities.Count >= comp.AvailableAbilities.Count)
        {
            _popups.PopupPredicted(Loc.GetString("shadowling-collective-mind-ascend"), uid, uid, PopupType.Medium);
            return;
        }

        comp.AmountOfThralls = sling.Thralls.Count;
        var thrallsRemaining = comp.ThrallsRequiredForAscension - comp.AmountOfThralls; // aka Thralls required for ascension

        if (thrallsRemaining < 0)
            thrallsRemaining = 0;

        var abiltiesAddedCount = 0;

        // Can we gain this power?
        foreach (var unlock in comp.AvailableAbilities)
        {
            if (comp.UnlockedAbilities.Contains(unlock))
                continue;

            var proto = _protoMan.Index(unlock);

            if (comp.AmountOfThralls < proto.UnlockAtThralls)
                continue;

            if (proto.AddComponents != null)
                EntityManager.AddComponents(args.Performer, proto.AddComponents);
            if (proto.RemoveComponents != null)
                EntityManager.RemoveComponents(args.Performer, proto.RemoveComponents);

            ++abiltiesAddedCount;
            comp.UnlockedAbilities.Add(unlock);
        }

        if (abiltiesAddedCount > 0)
        {
            _popups.PopupPredicted(
                Loc.GetString("shadowling-collective-mind-success", ("thralls", thrallsRemaining)),
                uid,
                uid,
                PopupType.Medium);

            var effectEnt = PredictedSpawnAtPosition(comp.CollectiveMindEffect, Transform(uid).Coordinates);
            _transform.SetParent(effectEnt, uid);
        }
        else
        {
            _popups.PopupPredicted(Loc.GetString("shadowling-collective-mind-failure", ("thralls", thrallsRemaining)),
                uid,
                uid,
                PopupType.Medium);
            return;
        }

        // Stun starts here.
        // Scales with amount of abilities added.
        // If no abilities were added, nothing happens as seen from the return statement above.
        foreach (var thrall in sling.Thralls)
        {
            if (!HasComp<StatusEffectsComponent>(thrall))
                return;

            _stun.TryParalyze(thrall, TimeSpan.FromSeconds(comp.BaseStunTime * abiltiesAddedCount + 1), false);
        }

        args.Handled = true;
    }
}
