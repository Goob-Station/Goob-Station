using System.Linq;
using Content.Server.Chat.Systems;
using Content.Shared._Goobstation.MartialArts;
using Content.Shared._Goobstation.MartialArts.Events;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Reflect;
using Robust.Shared.Audio;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.MartialArts;
public sealed partial class MartialArtsSystem
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
        if(ent.Comp.Used)
            return;
        if (ent.Comp.UseAgainTime == TimeSpan.Zero)
        {
            CarpScrollDelay(ent,args.User);
            return;
        }

        if (_timing.CurTime < ent.Comp.UseAgainTime)
        {
            _popupSystem.PopupEntity("The journey of a thousand miles begins with one step, and the path of wisdom is traveled slowly, one lesson at a time.", ent, args.User, PopupType.MediumCaution); // localize
            return;
        }

        switch (ent.Comp.Stage)
        {
            case < 3:
                CarpScrollDelay(ent,args.User);
                break;
            case >= 3:
                if (!CheckGrant(ent.Comp, args.User))
                    return;
                var martialArts = EnsureComp<MartialArtsKnowledgeComponent>(args.User);
                var userReflect = EnsureComp<ReflectComponent>(args.User);
                LoadPrototype(args.User, martialArts, ent.Comp.MartialArtsForm);
                martialArts.Blocked = false;
                userReflect.ReflectProb = 1;
                userReflect.Spread = 75;
                userReflect.OtherTypeReflectProb = 0.25f;
                ent.Comp.Used = true;
                _popupSystem.PopupEntity("You are now a master of the Way of the Sleeping Carp.", ent, args.User, PopupType.LargeCaution); // localize // localize
                return;
        }
    }

    private void CarpScrollDelay(Entity<GrantSleepingCarpComponent> ent, EntityUid user)
    {
        var time = new Random().Next(1, 2); // set to comp variables
        ent.Comp.UseAgainTime = _timing.CurTime + TimeSpan.FromSeconds(time);
        ent.Comp.Stage++;
        _popupSystem.PopupEntity("You have taken one step closer to becoming a master of the Way of the Sleeping Carp.", ent, user,  PopupType.Medium); // localize
    }
    #endregion

    #region Combo Methods
    private void OnSleepingCarpGnashing(Entity<CanPerformComboComponent> ent,
        ref SleepingCarpGnashingTeethPerformedEvent args)
    {
        if (!TryUseMartialArt(ent, MartialArtsForms.SleepingCarp, out var target, out _))
            return;

        if(!TryComp<MartialArtsKnowledgeComponent>(ent.Owner, out var knowledgeComponent))
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Slash", 20 + ent.Comp.ConsecutiveGnashes * 5) ;
        _damageable.TryChangeDamage(target, damage, origin: ent);
        ent.Comp.ConsecutiveGnashes++;
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg"), target);
        if (TryComp<RequireProjectileTargetComponent>(target, out var standing)
            && !standing.Active)
        {
            var saying =
                knowledgeComponent.RandomSayings.ElementAt(
                    _random.Next(knowledgeComponent.RandomSayings.Count));
            _chat.TrySendInGameICMessage(ent, Loc.GetString(saying), InGameICChatType.Speak, false);
        }
        else
        {
            var saying =
                knowledgeComponent.RandomSayingsDowned.ElementAt(_random.Next(knowledgeComponent.RandomSayingsDowned.Count));
            _chat.TrySendInGameICMessage(ent, Loc.GetString(saying) , InGameICChatType.Speak, false);
        }
    }

    private void OnSleepingCarpKneeHaul(Entity<CanPerformComboComponent> ent,
        ref SleepingCarpKneeHaulPerformedEvent args)
    {
        if (!TryUseMartialArt(ent, MartialArtsForms.SleepingCarp, out var target, out var downed))
            return;

        if (downed)
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 10);
        _damageable.TryChangeDamage(target, damage, origin: ent);
        _stamina.TakeStaminaDamage(target, 60f);
        _stun.TryParalyze(target, TimeSpan.FromSeconds(6), true);
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
    }
    private void OnSleepingCarpCrashingWaves(Entity<CanPerformComboComponent> ent, ref SleepingCarpCrashingWavesPerformedEvent args)
    {
        if (!TryUseMartialArt(ent, MartialArtsForms.SleepingCarp, out var target, out var downed))
            return;

        if (downed)
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 5);
        _damageable.TryChangeDamage(target, damage, origin: ent);
        var mapPos = _transform.GetMapCoordinates(ent).Position;
        var hitPos = _transform.GetMapCoordinates(target).Position;
        var dir = hitPos - mapPos;
        dir *= 1f / dir.Length();
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _grabThrowing.Throw(target, ent, dir, 25f, damage, damage);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit2.ogg"), target);
    }

    #endregion
}
