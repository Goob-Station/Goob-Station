using Content.Shared._Goobstation.MartialArts.Components;
using Content.Shared._Goobstation.MartialArts.Events;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio;

namespace Content.Shared._Goobstation.MartialArts;

public partial class SharedMartialArtsSystem
{
    private void InitializeCorporateJudo()
    {
        SubscribeLocalEvent<CanPerformComboComponent, JudoThrowPerformedEvent>(OnJudoThrow);
        SubscribeLocalEvent<CanPerformComboComponent, JudoEyePokePerformedEvent>(OnJudoEyepoke);
        SubscribeLocalEvent<CanPerformComboComponent, JudoArmbarPerformedEvent>(OnJudoArmbar);

        SubscribeLocalEvent<GrantCorporateJudoComponent, ClothingGotEquippedEvent>(OnGrantCorporateJudo);
        SubscribeLocalEvent<GrantCorporateJudoComponent, ClothingGotUnequippedEvent>(OnRemoveCorporateJudo);
        //SubscribeLocalEvent<CanPerformComboComponent, JudoGoldenBlastPerformedEvent>(OnJudoGoldenBlast); -- rework
        // Wheel throw
        // Discombobulate
    }

    #region Generic Methods

    private void OnGrantCorporateJudo(Entity<GrantCorporateJudoComponent> ent, ref ClothingGotEquippedEvent args)
    {
        if (!_netManager.IsServer)
            return;

        var user = args.Wearer;
        TryGrant(ent.Comp, user);
    }

    private void OnRemoveCorporateJudo(Entity<GrantCorporateJudoComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        var user = args.Wearer;
        if (!TryComp<MartialArtsKnowledgeComponent>(user, out var martialArtsKnowledge))
            return;

        if (martialArtsKnowledge.MartialArtsForm != MartialArtsForms.CorporateJudo)
            return;

        RemComp<MartialArtsKnowledgeComponent>(user);
        RemComp<CanPerformComboComponent>(user);
    }

    #endregion

    #region Combo Methods

    private void OnJudoThrow(Entity<CanPerformComboComponent> ent, ref JudoThrowPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto.MartialArtsForm, out var target, out var downed)
            || downed)
            return;

        _stun.TryKnockdown(target, TimeSpan.FromSeconds(proto.ParalyzeTime), false);
        _stamina.TakeStaminaDamage(target, proto.StaminaDamage);
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
    }

    private void OnJudoEyepoke(Entity<CanPerformComboComponent> ent, ref JudoEyePokePerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto.MartialArtsForm, out var target, out _))
            return;

        if (!TryComp(target, out StatusEffectsComponent? status))
            return;

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
        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
    }

    private void OnJudoArmbar(Entity<CanPerformComboComponent> ent, ref JudoArmbarPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto.MartialArtsForm, out var target, out var downed))
            return;

        switch (downed)
        {
            case false:
                var item = _hands.GetActiveItem(target);
                if (item != null)
                    _hands.TryDrop(target, item.Value);
                break;
            case true:
                _stamina.TakeStaminaDamage(target, proto.StaminaDamage);
                _stun.TryKnockdown(target, TimeSpan.FromSeconds(proto.ParalyzeTime), false);
                break;
        }

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
    }

    /* Pending Implement
    private void OnJudoGoldenBlast(Entity<CanPerformComboComponent> ent, ref JudoGoldenBlastPerformedEvent args)
    {
        if (!TryUseMartialArt(ent, MartialArtsForms.CorporateJudo, out var target, out var downed))
            return;

        if (downed)
            return;

        _stun.TryParalyze(target, TimeSpan.FromSeconds(30), false);
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
    }
    */

    #endregion
}
