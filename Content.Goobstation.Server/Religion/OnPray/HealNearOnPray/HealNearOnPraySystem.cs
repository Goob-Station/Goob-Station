// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.Religion.Nullrod;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Microsoft.CodeAnalysis.Operations;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Server.OnPray.HealNearOnPray;

public sealed partial class HealNearOnPraySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly ExamineSystemShared _occlusion = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HealNearOnPrayComponent, AlternatePrayEvent>(OnPray);
    }

    private void OnPray(EntityUid uid, HealNearOnPrayComponent comp, ref AlternatePrayEvent args)
    {
        var lookup = _lookup.GetEntitiesInRange(args.User, comp.Range);
        var canTarget = new HashSet<EntityUid>(lookup
            .Where(entity => entity != null && _occlusion.InRangeUnOccluded(uid, entity, comp.Range))
            .Select(entity => entity));

        foreach (var entity in canTarget.Where(HasComp<MobStateComponent>))
        {
            if (_mobState.IsDead(entity)
                || HasComp<SiliconComponent>(entity))
                continue;

            var ev = new DamageUnholyEvent(entity, args.User);
            RaiseLocalEvent(entity, ref ev);

            if (ev.ShouldTakeHoly)
            {
                _damageable.TryChangeDamage(entity, comp.Damage, targetPart: TargetBodyPart.All, splitDamage: SplitDamageBehavior.SplitEnsureAll);
                Spawn(comp.DamageEffect, Transform(entity).Coordinates);
                _audio.PlayPvs(comp.SizzleSoundPath, entity, new AudioParams(-2f, 1f, SharedAudioSystem.DefaultSoundRange, 1f, false, 0f)); //This should be safe to keep in the loop as this sound will never consistently play on multiple entities.
            }
            else
            {
                _damageable.TryChangeDamage(entity, comp.Healing, targetPart: TargetBodyPart.All, ignoreBlockers: true, splitDamage: SplitDamageBehavior.SplitEnsureAll);
                Spawn(comp.HealEffect, Transform(entity).Coordinates);
            }
        }
        _audio.PlayPvs(comp.HealSoundPath, uid, new AudioParams(-2f, 1f, SharedAudioSystem.DefaultSoundRange, 1f, false, 0f)); //Played outside the loop once at the source of the damage to prevent repeated sound-stacking.
    }
}
