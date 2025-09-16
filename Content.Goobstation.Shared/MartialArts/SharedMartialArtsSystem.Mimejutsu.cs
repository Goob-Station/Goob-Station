using System.Linq;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared._Shitmed.Medical.Surgery.Traumas;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Speech.Muting;
using Robust.Shared.Random;
using Robust.Shared.Audio;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.MartialArts;

public partial class SharedMartialArtsSystem
{

    private void InitializeMimejutsu()
    {
        SubscribeLocalEvent<CanPerformComboComponent, MimejutsuSilentExecutionPerformedEvent>(OnMimejutsuSilentExecution);
        SubscribeLocalEvent<CanPerformComboComponent, MimejutsuMimechucksPerformedEvent>(OnMimejutsuMimechucks);
        SubscribeLocalEvent<CanPerformComboComponent, MimejutsuSilencerPerformedEvent>(OnMimejutsuSilencer);
        SubscribeLocalEvent<CanPerformComboComponent, MimejutsuSilentPalmPerformedEvent>(OnMimejutsuSilentPalm);

        SubscribeLocalEvent<MartialArtsKnowledgeComponent, CanDoMimejutsuEvent>(OnMimejutsuCheck);

        SubscribeLocalEvent<GrantMimejutsuComponent, UseInHandEvent>(OnGrantCQCUse);
    }

    private void OnMimejutsuCheck(Entity<MartialArtsKnowledgeComponent> ent, ref CanDoMimejutsuEvent args)
    {
        if (args.Handled)
            return;

        if (!ent.Comp.Blocked)
        {
            args.Handled = true;
            return;
        }
    }

    private void OnMimejustuAttackPerformed(Entity<MartialArtsKnowledgeComponent> ent, ref ComboAttackPerformedEvent args)
    {
        if (args.Weapon != args.Performer || args.Target == args.Performer)
            return;

        switch (args.Type)
        {
            case ComboAttackType.Disarm:
                _stamina.TakeStaminaDamage(args.Target, 25f, applyResistances: true);
                break;
            case ComboAttackType.Harm:
                if (!TryComp(args.Target, out StatusEffectsComponent? status))
                    break;
                var random = new Random((int) _timing.CurTick.Value + (int) GetNetEntity(ent));
                if (_random.Prob(0.30f))
                {
                    _stun.TrySlowdown(args.Target, TimeSpan.FromSeconds(2), true, 0.5f, 0.5f, status);
                    ComboPopup(ent, args.Target, "Silent punch");
                }
                break;
        }
    }

    private void OnMimejutsuSilentExecution(Entity<CanPerformComboComponent> ent, ref MimejutsuSilentExecutionPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto) ||
            !TryUseMartialArt(ent, proto, out var target, out _) ||
            !TryComp(target, out BodyComponent? body) ||
            !TryComp(ent, out TargetingComponent? targeting))
            return;

        var (partType, symmetry) = _body.ConvertTargetBodyPart(targeting.Target);
        var targetedBodyPart = _body.GetBodyChildrenOfType(target, partType, body, symmetry)
            .ToList()
            .FirstOrNull();

        if (targetedBodyPart == null ||
            !TryComp(targetedBodyPart.Value.Id, out WoundableComponent? woundable) ||
            woundable.Bone.ContainedEntities.FirstOrNull() is not { } bone ||
            !TryComp(bone, out BoneComponent? boneComp))
            return;

        if (boneComp.BoneSeverity == BoneSeverity.Broken)
            DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);

        else
            _trauma.ApplyDamageToBone(bone, boneComp.BoneIntegrity, boneComp);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit2.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnMimejutsuMimechucks(Entity<CanPerformComboComponent> ent, ref MimejutsuMimechucksPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _))
            return;

        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, source: ent, applyResistances: true);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnMimejutsuSilencer(Entity<CanPerformComboComponent> ent, ref MimejutsuSilencerPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _)
            || !TryComp(target, out StatusEffectsComponent? status))
            return;

        _stun.TrySlowdown(target, TimeSpan.FromSeconds(3), true, 0.5f, 0.5f, status);

        _status.TryAddStatusEffect<MutedComponent>(target, "Muted", TimeSpan.FromSeconds(10), true);

        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, applyResistances: true);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnMimejutsuSilentPalm(Entity<CanPerformComboComponent> ent, ref MimejutsuSilentPalmPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _)
            || !TryComp(target, out StatusEffectsComponent? status))
            return;

        var mapPos = _transform.GetMapCoordinates(ent).Position;
        var hitPos = _transform.GetMapCoordinates(target).Position;
        var dir = hitPos - mapPos;
        dir *= 3f / dir.Length();


        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);

        _grabThrowing.Throw(target, ent, dir, proto.ThrownSpeed);
        _stun.TrySlowdown(target, TimeSpan.FromSeconds(1), true, 0.5f, 0.5f, status);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit2.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }
}