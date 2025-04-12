// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Shared.Damage;
using Content.Shared.Whitelist;
using Robust.Shared.Random;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class BreakWindowArtifactSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DamageNearbyArtifactComponent, ArtifactActivatedEvent>(OnActivated);
    }

    private void OnActivated(EntityUid uid, DamageNearbyArtifactComponent component, ArtifactActivatedEvent args)
    {
        var ents = _lookup.GetEntitiesInRange(uid, component.Radius);
        if (args.Activator != null)
            ents.Add(args.Activator.Value);
        foreach (var ent in ents)
        {
            if (_whitelistSystem.IsWhitelistFail(component.Whitelist, ent))
                continue;

            if (!_random.Prob(component.DamageChance))
                return;

            _damageable.TryChangeDamage(ent, component.Damage, component.IgnoreResistances);
        }
    }
}