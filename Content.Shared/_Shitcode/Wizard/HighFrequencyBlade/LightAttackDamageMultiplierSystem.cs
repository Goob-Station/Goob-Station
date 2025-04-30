// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Goobstation.Wizard.HighFrequencyBlade;

public sealed class LightAttackDamageMultiplierSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LightAttackDamageMultiplierComponent, MeleeHitEvent>(OnHit);
    }

    private void OnHit(Entity<LightAttackDamageMultiplierComponent> ent, ref MeleeHitEvent args)
    {
        var comp = ent.Comp;

        if (!args.IsHit || args.Direction != null || args.HitEntities.Count == 0 || comp.Multiplier < 1f)
            return;

        args.BonusDamage += args.BaseDamage * (comp.Multiplier - 1f);

        _audio.PlayPredicted(comp.ExtraSound, ent, args.User);
    }
}