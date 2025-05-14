// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.OnPray.HealNearOnPray;

public sealed partial class HealNearOnPraySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HealNearOnPrayComponent, AlternatePrayEvent>(OnPray);
    }

    private void OnPray(EntityUid uid, HealNearOnPrayComponent comp, ref AlternatePrayEvent args)
    {
        var lookup = _lookup.GetEntitiesInRange(args.User, comp.Range);

        foreach (var entity in lookup.Where(HasComp<MobStateComponent>))
        {
            var ev = new DamageUnholyEvent(entity, args.User);
            RaiseLocalEvent(entity, ref ev);

            if (ev.ShouldTakeHoly)
            {
                _damageable.TryChangeDamage(entity, comp.Damage, targetPart: TargetBodyPart.All, ignoreBlockers: true);
                _audio.PlayPvs(comp.SizzleSoundPath, entity);
                Spawn(comp.DamageEffect, Transform(entity).Coordinates);
            }
            else
            {
                _damageable.TryChangeDamage(entity, comp.Healing * 17f, targetPart: TargetBodyPart.All, ignoreBlockers: true);
                _audio.PlayPvs(comp.HealSoundPath, entity);
                Spawn(comp.HealEffect, Transform(entity).Coordinates);
            }
        }
    }
}
