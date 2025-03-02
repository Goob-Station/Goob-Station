using Content.Shared._Goobstation.MartialArts;
using Content.Shared._Goobstation.MartialArts.Events;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Hands;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio;

namespace Content.Server._Goobstation.MartialArts;
public sealed partial class MartialArtsSystem
{
    private void InitializeCorporateJudo()
    {
        SubscribeLocalEvent<CanPerformComboComponent, JudoThrowPerformedEvent>(OnJudoThrow);
        SubscribeLocalEvent<CanPerformComboComponent, JudoEyePokePerformedEvent>(OnJudoEyepoke);
        SubscribeLocalEvent<CanPerformComboComponent, JudoArmbarPerformedEvent>(OnJudoArmbar);
        SubscribeLocalEvent<MartialArtBlockedComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<GrantCorporateJudoComponent, ClothingGotEquippedEvent>(OnGrantCorporateJudo);
        SubscribeLocalEvent<GrantCorporateJudoComponent, ClothingGotUnequippedEvent>(OnRemoveCorporateJudo);
        //SubscribeLocalEvent<CanPerformComboComponent, JudoGoldenBlastPerformedEvent>(OnJudoGoldenBlast); -- rework
        // Wheel throw
        // Discombobulate
    }

    #region  Methods
    private void OnGrantCorporateJudo(Entity<GrantCorporateJudoComponent> ent, ref ClothingGotEquippedEvent args)
    {
        var user = args.Wearer;
        if (!CheckGrant(ent.Comp, user))
            return;
        var martialArts = EnsureComp<MartialArtsKnowledgeComponent>(user);
        LoadPrototype(user, martialArts, ent.Comp.MartialArtsForm);
        martialArts.Blocked = false;
    }

    private void OnRemoveCorporateJudo(Entity<GrantCorporateJudoComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        var user = args.Wearer;
        RemComp<MartialArtsKnowledgeComponent>(user);
        RemComp<CanPerformComboComponent>(user);
    }

    private void OnGotEquippedHand(Entity<MartialArtBlockedComponent> ent, ref GotEquippedHandEvent args)
    {
        if (!TryComp<MartialArtsKnowledgeComponent>(args.User, out  var knowledgeComp))
            return;
        if(knowledgeComp.MartialArtsForm != ent.Comp.Form)
            return;

        _hands.TryDrop(args.User);
        _popupSystem.PopupEntity("Corporate Judo strictly forbids the use of stun batons.", ent, PopupType.MediumCaution);
    }
    #endregion

    #region Combo Methods
    private void OnJudoThrow(Entity<CanPerformComboComponent> ent, ref JudoThrowPerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;
        if (!CheckCanUseMartialArt(ent, MartialArtsForms.CorporateJudo))
            return;

        var target = ent.Comp.CurrentTarget.Value;

        if (TryComp<RequireProjectileTargetComponent>(target, out var downed) && downed.Active)
            return;

        _stun.TryParalyze(target, TimeSpan.FromSeconds(7), false);
        _stamina.TakeStaminaDamage(target, 25f);
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
    }
    private void OnJudoEyepoke(Entity<CanPerformComboComponent> ent, ref JudoEyePokePerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(ent, MartialArtsForms.CorporateJudo))
            return;
        var target = ent.Comp.CurrentTarget.Value;

        if (TryComp<RequireProjectileTargetComponent>(target, out var downed) && downed.Active)
            return;

        if (!TryComp(target, out StatusEffectsComponent? status))
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 10);
        _status.TryAddStatusEffect<TemporaryBlindnessComponent>(target,
            "TemporaryBlindness",
            TimeSpan.FromSeconds(2),
            true,
            status);
        _status.TryAddStatusEffect<BlurryVisionComponent>(target,
            "BlurryVision",
            TimeSpan.FromSeconds(5),
            false,
            status);
        _damageable.TryChangeDamage(target, damage, origin: ent);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
    }
    private void OnJudoArmbar(Entity<CanPerformComboComponent> ent, ref JudoArmbarPerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(ent, MartialArtsForms.CorporateJudo))
            return;

        var target = ent.Comp.CurrentTarget.Value;

        if (!TryComp<RequireProjectileTargetComponent>(target, out var downed))
            return;

        switch (downed.Active)
        {
            case false:
                var item = _hands.GetActiveItem(target);
                if (item != null)
                    _hands.TryDrop(target, item.Value);
                break;
            case true:
                _stamina.TakeStaminaDamage(target, 45f);
                _stun.TryParalyze(target, TimeSpan.FromSeconds(5), false);
                _popupSystem.PopupEntity("You were placed in an arm bar", target);
                break;
        }
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
    }

    private void OnJudoGoldenBlast(Entity<CanPerformComboComponent> ent, ref JudoGoldenBlastPerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;
        if (!CheckCanUseMartialArt(ent, MartialArtsForms.CorporateJudo))
            return;

        var target = ent.Comp.CurrentTarget.Value;

        if (TryComp<RequireProjectileTargetComponent>(target, out var downed) && downed.Active)
            return;

        _stun.TryParalyze(target, TimeSpan.FromSeconds(30), false);
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
    }
    #endregion
}
