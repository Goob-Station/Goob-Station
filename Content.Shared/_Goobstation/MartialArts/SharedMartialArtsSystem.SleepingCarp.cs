using System.Linq;
using Content.Shared._Goobstation.MartialArts.Components;
using Content.Shared._Goobstation.MartialArts.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Reflect;
using Robust.Shared.Audio;

namespace Content.Shared._Goobstation.MartialArts;

public partial class SharedMartialArtsSystem
{
    private void InitializeSleepingCarp()
    {
        SubscribeLocalEvent<CanPerformComboComponent, SleepingCarpGnashingTeethPerformedEvent>(OnSleepingCarpGnashing);
        SubscribeLocalEvent<CanPerformComboComponent, SleepingCarpKneeHaulPerformedEvent>(OnSleepingCarpKneeHaul);
        SubscribeLocalEvent<CanPerformComboComponent, SleepingCarpCrashingWavesPerformedEvent>(OnSleepingCarpCrashingWaves);

        SubscribeLocalEvent<GrantSleepingCarpComponent, UseInHandEvent>(OnGrantSleepingCarp);
    }

    #region Generic Methods

    private void OnGrantSleepingCarp(Entity<GrantSleepingCarpComponent> ent, ref UseInHandEvent args)
    {
        if (!_netManager.IsServer)
            return;

        if (ent.Comp.Used)
            return;
        if (ent.Comp.UseAgainTime == TimeSpan.Zero)
        {
            CarpScrollDelay(ent, args.User);
            return;
        }

        if (_timing.CurTime < ent.Comp.UseAgainTime)
        {
            _popupSystem.PopupEntity(
                Loc.GetString("carp-scroll-waiting"),
                ent,
                args.User,
                PopupType.MediumCaution);
            return;
        }

        switch (ent.Comp.Stage)
        {
            case < 3:
                CarpScrollDelay(ent, args.User);
                break;
            case >= 3:
                if (!TryGrant(ent.Comp, args.User))
                    return;
                var userReflect = EnsureComp<ReflectComponent>(args.User);
                userReflect.ReflectProb = 1;
                userReflect.Spread = 60;
                userReflect.OtherTypeReflectProb = 0.25f;
                ent.Comp.Used = true;
                _popupSystem.PopupEntity(
                    Loc.GetString("carp-scroll-complete"),
                    ent,
                    args.User,
                    PopupType.LargeCaution);
                return;
        }
    }

    private void CarpScrollDelay(Entity<GrantSleepingCarpComponent> ent, EntityUid user)
    {
        var time = new System.Random().Next(ent.Comp.MinUseDelay, ent.Comp.MaxUseDelay);
        ent.Comp.UseAgainTime = _timing.CurTime + TimeSpan.FromSeconds(time);
        ent.Comp.Stage++;
        _popupSystem.PopupEntity(
            Loc.GetString("carp-scroll-advance"),
            ent,
            user,
            PopupType.Medium);
    }

    #endregion

    #region Combo Methods

    private void OnSleepingCarpGnashing(Entity<CanPerformComboComponent> ent,
        ref SleepingCarpGnashingTeethPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto.MartialArtsForm, out var target, out _))
            return;

        if (!TryComp<MartialArtsKnowledgeComponent>(ent.Owner, out var knowledgeComponent))
            return;
        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage + ent.Comp.ConsecutiveGnashes * 5, out _);
        ent.Comp.ConsecutiveGnashes++;
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg"), target);
        if (TryComp<RequireProjectileTargetComponent>(target, out var standing)
            && !standing.Active)
        {
            var saying =
                knowledgeComponent.RandomSayings.ElementAt(
                    _random.Next(knowledgeComponent.RandomSayings.Count));
            var ev = new SleepingCarpSaying(saying);
            RaiseLocalEvent(ent, ev);
        }
        else
        {
            var saying =
                knowledgeComponent.RandomSayingsDowned.ElementAt(
                    _random.Next(knowledgeComponent.RandomSayingsDowned.Count));
            var ev = new SleepingCarpSaying(saying);
            RaiseLocalEvent(ent, ev);
        }
    }

    private void OnSleepingCarpKneeHaul(Entity<CanPerformComboComponent> ent,
        ref SleepingCarpKneeHaulPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto.MartialArtsForm, out var target, out var downed)
            || downed)
            return;

        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);
        _stamina.TakeStaminaDamage(target, proto.StaminaDamage);
        _stun.TryParalyze(target, TimeSpan.FromSeconds(proto.ParalyzeTime), true);
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
    }

    private void OnSleepingCarpCrashingWaves(Entity<CanPerformComboComponent> ent,
        ref SleepingCarpCrashingWavesPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto.MartialArtsForm, out var target, out var downed)
            || downed)
            return;

        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out var damage);
        var mapPos = _transform.GetMapCoordinates(ent).Position;
        var hitPos = _transform.GetMapCoordinates(target).Position;
        var dir = hitPos - mapPos;
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _grabThrowing.Throw(target, ent, dir, proto.ThrownSpeed, proto.StaminaDamage / 2, damage);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit2.ogg"), target);
        ComboPopup(ent, target, proto.Name);
    }

    #endregion
}
