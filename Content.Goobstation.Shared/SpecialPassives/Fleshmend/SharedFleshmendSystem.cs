// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Atmos;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.SpecialPassives.Fleshmend.Components;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Alert;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;

namespace Content.Goobstation.Shared.SpecialPassives.Fleshmend;

public sealed class SharedFleshmendSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly WoundSystem _wound = default!;

    private EntityQuery<DamageableComponent> _damageableQuery;
    private EntityQuery<MobStateComponent> _mobstateQuery;

    public override void Initialize()
    {
        base.Initialize();

        _damageableQuery = GetEntityQuery<DamageableComponent>();
        _mobstateQuery = GetEntityQuery<MobStateComponent>();

        SubscribeLocalEvent<FleshmendComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<FleshmendComponent, ComponentRemove>(OnRemoved);

        SubscribeLocalEvent<FleshmendComponent, MobStateChangedEvent>(OnMobStateChange);
    }

    private void OnMapInit(Entity<FleshmendComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        if (ent.Comp.Duration.HasValue)
            ent.Comp.MaxDuration = _timing.CurTime + TimeSpan.FromSeconds((double) ent.Comp.Duration);

        if (_mobstateQuery.TryComp(ent, out var state))
            ent.Comp.Mobstate = state.CurrentState;

        Cycle(ent);
    }

    private void OnRemoved(Entity<FleshmendComponent> ent, ref ComponentRemove args)
    {
        if (!_netManager.IsClient) // it'll throw a warning otherwise
            RemoveFleshmendEffects(ent);

        if (ent.Comp.AlertId != null)
            _alerts.ClearAlert(ent, (ProtoId<AlertPrototype>) ent.Comp.AlertId); // incase there was still time left on removal
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<FleshmendComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.MaxDuration < _timing.CurTime
                && comp.Duration.HasValue) // assume it lasts forever otherwise
                RemCompDeferred<FleshmendComponent>(uid);

            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + comp.UpdateDelay;

            Cycle((uid, comp));
        }
    }

    private void Cycle(Entity<FleshmendComponent> ent)
    {
        if (!IsValidFireCheck(ent)
            || !IsValidMobstateCheck(ent))
            return;

        TryAddFleshmendEffects(ent);

        HealDamage(ent);
    }

    #region Event Handlers

    private void OnMobStateChange(Entity<FleshmendComponent> ent, ref MobStateChangedEvent args)
    {
        ent.Comp.Mobstate = args.NewMobState;
    }

    #endregion

    #region Helper Methods

    public readonly ProtoId<DamageGroupPrototype> BruteDamageGroup = "Brute";
    public readonly ProtoId<DamageGroupPrototype> BurnDamageGroup = "Burn";

    private void HealDamage(Entity<FleshmendComponent> ent)
    {
        // the dmg groups
        var bruteTypes = _proto.Index(BruteDamageGroup);
        var burnTypes = _proto.Index(BurnDamageGroup);

        // set the amount of healing depending on non-zero damage types present
        if (!_damageableQuery.TryComp(ent, out var damage))
            return;

        var bruteDiv =
            bruteTypes.DamageTypes.Count(type =>
            damage.Damage.DamageDict.GetValueOrDefault(type)
            != FixedPoint2.Zero);

        var burnDiv =
            burnTypes.DamageTypes.Count(type =>
            damage.Damage.DamageDict.GetValueOrDefault(type)
            != FixedPoint2.Zero);

        var bruteHealAmount = ent.Comp.BruteHeal / bruteDiv;
        var burnHealAmount = ent.Comp.BurnHeal / burnDiv;

        var healSpec = new DamageSpecifier();

        foreach (var brute in bruteTypes.DamageTypes)
            healSpec.DamageDict.Add(brute, bruteHealAmount);

        foreach (var burn in burnTypes.DamageTypes)
            healSpec.DamageDict.Add(burn, burnHealAmount);

        healSpec.DamageDict.Add("Asphyxiation", ent.Comp.AsphyxHeal);

        // heal the damage
        _dmg.TryChangeDamage(ent, healSpec, true, false, targetPart: TargetBodyPart.All, splitDamage: SplitDamageBehavior.SplitEnsureAllOrganic);

        // heal bleeding and restore blood
        _bloodstream.TryModifyBleedAmount(ent.Owner, ent.Comp.BleedingAdjust);
        //_wound.TryHealMostSevereBleedingWoundables(ent, -ent.Comp.BleedingAdjust, out _); - moved to trymodifybleedamount
        _bloodstream.TryModifyBloodLevel(ent.Owner, ent.Comp.BloodLevelAdjust);
    }

    private void TryAddFleshmendEffects(Entity<FleshmendComponent> ent)
    {
        if (ent.Comp.ResPath != ResPath.Empty
            && ent.Comp.EffectState != null)
        {
            var vfx = EnsureComp<FleshmendEffectComponent>(ent);
            vfx.ResPath = ent.Comp.ResPath;
            vfx.EffectState = ent.Comp.EffectState;
            Dirty(ent, vfx);
        }

        if (ent.Comp.PassiveSound != null
            && ent.Comp.SoundSource == null)
            DoFleshmendSound(ent);
    }

    private void RemoveFleshmendEffects(Entity<FleshmendComponent> ent)
    {
        RemComp<FleshmendEffectComponent>(ent);

        StopFleshmendSound(ent);
    }

    private void DoFleshmendSound(Entity<FleshmendComponent> ent)
    {
        var audioParams = AudioParams.Default.WithLoop(true).WithVolume(-3f);
        var source = _audio.PlayPredicted(ent.Comp.PassiveSound, ent, null, audioParams);
        ent.Comp.SoundSource = source?.Entity;
    }

    private void StopFleshmendSound(Entity<FleshmendComponent> ent)
    {
        _audio.Stop(ent.Comp.SoundSource);
        ent.Comp.SoundSource = null;
    }

    private bool IsValidFireCheck(Entity<FleshmendComponent> ent)
    {
        var fireEv = new GetFireStateEvent();
        RaiseLocalEvent(ent, ref fireEv);

        if (fireEv.OnFire
            && !ent.Comp.IgnoreFire)
        {
            RemoveFleshmendEffects(ent);
            return false;
        }

        return true;
    }

    private bool IsValidMobstateCheck(Entity<FleshmendComponent> ent)
    {
        if (ent.Comp.Mobstate == MobState.Dead
            && !ent.Comp.WorkWhileDead)
        {
            RemComp<FleshmendComponent>(ent);
            return false; // seems unecessary, but this stops the rest of the logic from running and prevents the vfx/sfx from persisting
        }

        return true;
    }

    #endregion
}
