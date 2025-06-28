// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Server.Atmos.Components;
using Content.Server.Body.Systems;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Changeling;

public sealed partial class FleshmendSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly WoundSystem _wound = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FleshmendComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FleshmendComponent, ComponentRemove>(OnRemoved);
    }

    private void OnStartup(Entity<FleshmendComponent> ent, ref ComponentStartup args)
    {
        if (ent.Comp.DoVisualEffect)
            EnsureComp<FleshmendEffectComponent>(ent);

        if (ent.Comp.PassiveSound != null)
            DoFleshmendSound(ent);

        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        Cycle(ent);
    }

    private void OnRemoved(Entity<FleshmendComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.DoVisualEffect)
            RemComp<FleshmendEffectComponent>(ent);

        if (ent.Comp.PassiveSound != null)
            StopFleshmendSound(ent);
    }

    private void DoFleshmendSound(Entity<FleshmendComponent> ent)
    {
        var audioParams = AudioParams.Default.WithLoop(true).WithVolume(-3f);
        var source = _audio.PlayPvs(ent.Comp.PassiveSound, ent, audioParams);
        ent.Comp.SoundSource = source?.Entity;
    }

    private void StopFleshmendSound(Entity<FleshmendComponent> ent)
    {
        _audio.Stop(ent.Comp.SoundSource);
        ent.Comp.SoundSource = null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<FleshmendComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!uid.IsValid()
                || _timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + comp.UpdateDelay;

            Cycle((uid, comp));
        }
    }

    public readonly ProtoId<DamageGroupPrototype> BruteDamageGroup = "Brute";
    public readonly ProtoId<DamageGroupPrototype> BurnDamageGroup = "Burn";
    private void Cycle(Entity<FleshmendComponent> ent)
    {
        if (TryComp<FlammableComponent>(ent, out var flam)
            && flam.OnFire
            && !ent.Comp.IgnoreFire)
        {
            if (ent.Comp.DoVisualEffect)
                RemComp<FleshmendEffectComponent>(ent);

            if (ent.Comp.PassiveSound != null)
                StopFleshmendSound(ent);

            return;
        }
        else
        {
            if (ent.Comp.DoVisualEffect)
                EnsureComp<FleshmendEffectComponent>(ent);

            if (ent.Comp.PassiveSound != null
                && ent.Comp.SoundSource == null)
                DoFleshmendSound(ent);
        }

        // the dmg groups
        var bruteTypes = _proto.Index(BruteDamageGroup);
        var burnTypes = _proto.Index(BurnDamageGroup);

        // nuke this whole section when EvenHealthChange or smth similar becomes real
        if (!TryComp<DamageableComponent>(ent, out var damage))
            return;

        var bruteMult = 1 +
            bruteTypes.DamageTypes.Count(type =>
            damage.Damage.DamageDict.GetValueOrDefault(type)
            == FixedPoint2.Zero);

        var burnMult = 1 +
            burnTypes.DamageTypes.Count(type =>
            damage.Damage.DamageDict.GetValueOrDefault(type)
            == FixedPoint2.Zero);

        var bruteHealAmount = ent.Comp.BruteHeal * bruteMult;
        var burnHealAmount = ent.Comp.BurnHeal * burnMult;
        //

        // the damage spec & damage specs for groups
        var bruteHeal = new DamageSpecifier(bruteTypes, bruteHealAmount);
        var burnHeal = new DamageSpecifier(burnTypes, burnHealAmount);

        var specific = new DamageSpecifier();
        specific.DamageDict.Add("Asphyxiation", ent.Comp.AsphyxHeal);

        // the groups (has to use several "TryChangeDamage"s unfortunately)
        _dmg.TryChangeDamage(ent, bruteHeal, true, false, targetPart: TargetBodyPart.All, splitDamage: SplitDamageBehavior.SplitEnsureAllOrganic);
        _dmg.TryChangeDamage(ent, burnHeal, true, false, targetPart: TargetBodyPart.All, splitDamage: SplitDamageBehavior.SplitEnsureAllOrganic);

        // specific damage types
        _dmg.TryChangeDamage(ent, specific, true, false, targetPart: TargetBodyPart.All, splitDamage: SplitDamageBehavior.SplitEnsureAllOrganic);

        // heal bleeding and restore blood
        _bloodstream.TryModifyBleedAmount(ent, ent.Comp.BleedingAdjust);
        _wound.TryHealMostSevereBleedingWoundables(ent, -ent.Comp.BleedingAdjust, out _);
        _bloodstream.TryModifyBloodLevel(ent, ent.Comp.BloodLevelAdjust);
    }
}
