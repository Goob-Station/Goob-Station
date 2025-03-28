using System.Linq;
using Content.Shared._Goobstation.MartialArts.Components;
using Content.Shared._Goobstation.MartialArts.Events;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.IdentityManagement;
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

        if (ent.Comp.MaximumUses == ent.Comp.CurrentUses)
        {
            _popupSystem.PopupEntity(Loc.GetString("cqc-fail-used", ("manual", Identity.Entity(ent, EntityManager))),
            args.User,
            args.User);
            return;
        }

        var studentComp = EnsureComp<SleepingCarpStudentComponent>(args.User);

        if (studentComp.UseAgainTime == TimeSpan.Zero)
        {
            CarpScrollDelay((args.User, studentComp));
            return;
        }

        if (_timing.CurTime < studentComp.UseAgainTime)
        {
            _popupSystem.PopupEntity(
                Loc.GetString("carp-scroll-waiting"),
                ent,
                args.User,
                PopupType.MediumCaution);
            return;
        }

        switch (studentComp.Stage)
        {
            case < 3:
                CarpScrollDelay((args.User, studentComp));
                break;
            case >= 3:
                if (!TryGrantMartialArt(args.User, ent.Comp))
                    return;
                var userReflect = EnsureComp<ReflectComponent>(args.User);
                userReflect.ReflectProb = 1;
                userReflect.Spread = 60;
                userReflect.OtherTypeReflectProb = 0.25f;
                _popupSystem.PopupEntity(
                    Loc.GetString("carp-scroll-complete"),
                    ent,
                    args.User,
                    PopupType.LargeCaution);
                ent.Comp.CurrentUses++;
                break;
        }

        if (ent.Comp.MaximumUses == ent.Comp.CurrentUses)
            return;
    }

    private void CarpScrollDelay(Entity<SleepingCarpStudentComponent> ent)
    {
        var time = new System.Random().Next(ent.Comp.MinUseDelay, ent.Comp.MaxUseDelay);
        ent.Comp.UseAgainTime = _timing.CurTime + TimeSpan.FromSeconds(time);
        ent.Comp.Stage++;
        _popupSystem.PopupEntity(
            Loc.GetString("carp-scroll-advance"),
            ent,
            ent,
            PopupType.Medium);
    }

    #endregion

    #region Combo Methods

    private void OnSleepingCarpGnashing(Entity<CanPerformComboComponent> ent,
        ref SleepingCarpGnashingTeethPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !_proto.TryIndex<MartialArtPrototype>(proto.MartialArtsForm.ToString(), out var martialArtProto)
            || !TryUseMartialArt(ent, proto.MartialArtsForm, out var target, out var downed))
            return;

        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage + ent.Comp.ConsecutiveGnashes * 5, out _);
        ent.Comp.ConsecutiveGnashes++;
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg"), target);
        if (!downed)
        {
            var saying =
                martialArtProto.RandomSayings.ElementAt(
                    _random.Next(martialArtProto.RandomSayings.Count));
            var ev = new SleepingCarpSaying(saying);
            RaiseLocalEvent(ent, ev);
        }
        else
        {
            var saying =
                martialArtProto.RandomSayingsDowned.ElementAt(
                    _random.Next(martialArtProto.RandomSayingsDowned.Count));
            var ev = new SleepingCarpSaying(saying);
            RaiseLocalEvent(ent, ev);
        }
    }

    private void OnSleepingCarpKneeHaul(Entity<CanPerformComboComponent> ent,
        ref SleepingCarpKneeHaulPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto.MartialArtsForm, out var target, out var downed))
            return;

        if (!downed)
        {
            DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);
            _stamina.TakeStaminaDamage(target, proto.StaminaDamage);
            _stun.TryKnockdown(target, TimeSpan.FromSeconds(proto.ParalyzeTime), true);
        }
        else
        {
            DoDamage(ent, target, proto.DamageType, proto.ExtraDamage / 2, out _);
            _stamina.TakeStaminaDamage(target, proto.StaminaDamage - 20);
            _hands.TryDrop(target);
        }
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
        _grabThrowing.Throw(target, ent, dir, proto.ThrownSpeed, damage);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit2.ogg"), target);
        ComboPopup(ent, target, proto.Name);
    }

    #endregion
}
