// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;
using Content.Shared.Damage;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Systems;

public sealed class ArtifactDamageTriggerSystem : EntitySystem
{
    [Dependency] private readonly ArtifactSystem _artifact = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ArtifactDamageTriggerComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(EntityUid uid, ArtifactDamageTriggerComponent component, DamageChangedEvent args)
    {
        if (!args.DamageIncreased)
            return;

        if (args.DamageDelta == null)
            return;

        foreach (var (type, amount) in args.DamageDelta.DamageDict)
        {
            if (component.DamageTypes != null && !component.DamageTypes.Contains(type))
                continue;

            component.AccumulatedDamage += (float) amount;
        }

        if (component.AccumulatedDamage >= component.DamageThreshold)
            _artifact.TryActivateArtifact(uid, args.Origin);
    }
}