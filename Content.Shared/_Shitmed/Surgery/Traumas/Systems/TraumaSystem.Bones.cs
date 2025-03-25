using System.Linq;
using Content.Shared._Shitmed.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Surgery.Wounds.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.FixedPoint;
using Content.Shared.Movement.Components;
using Robust.Shared.Audio;
using Robust.Shared.Random;

namespace Content.Shared._Shitmed.Surgery.Traumas.Systems;

public partial class TraumaSystem
{
    private const float BoneDamageSeverityThreshold = 9f;

    private void InitBones()
    {
        SubscribeLocalEvent<BoneComponent, BoneSeverityChangedEvent>(OnBoneSeverityChanged);
        SubscribeLocalEvent<BoneComponent, BoneSeverityPointChangedEvent>(OnBoneSeverityPointChanged);
    }

    #region Event handling

    private void OnBoneSeverityChanged(EntityUid uid, BoneComponent component, BoneSeverityChangedEvent args)
    {
        ApplyBoneDamageEffects(component);

        if (!TryComp<BodyPartComponent>(component.BoneWoundable, out var bodyPart)
            || bodyPart.Body == null || !TryComp<BodyComponent>(bodyPart.Body, out var body))
            return;

        ProcessLegsState(bodyPart.Body.Value, body);
    }

    private void OnBoneSeverityPointChanged(EntityUid uid, BoneComponent component, BoneSeverityPointChangedEvent args)
    {
        if (!TryComp<BodyPartComponent>(component.BoneWoundable, out var bodyPart) || bodyPart.Body == null)
            return;

        if (!_consciousness.TryGetNerveSystem(bodyPart.Body.Value, out var nerveSys))
            return;

        if (component.BoneWoundable == null)
            return;

        if (!_pain.TryChangePainModifier(nerveSys.Value,
                component.BoneWoundable.Value,
                "BoneDamage",
                args.SeverityDelta * _bonePainModifiers[component.BoneSeverity]))
        {
            _pain.TryAddPainModifier(nerveSys.Value,
                component.BoneWoundable.Value,
                "BoneDamage",
                args.SeverityDelta * _bonePainModifiers[component.BoneSeverity]);
        }
    }


    #endregion

    #region Public API

    public bool ApplyDamageToBone(EntityUid bone, FixedPoint2 severity, BoneComponent? boneComp = null)
    {
        if (!Resolve(bone, ref boneComp))
            return false;

        var newIntegrity = FixedPoint2.Clamp(boneComp.BoneIntegrity - severity, 0, boneComp.IntegrityCap);
        if (boneComp.BoneIntegrity == newIntegrity)
            return false;

        boneComp.BoneIntegrity = newIntegrity;
        CheckBoneSeverity(bone, boneComp);

        var ev = new BoneSeverityPointChangedEvent(bone, boneComp, boneComp.BoneIntegrity, severity);
        RaiseLocalEvent(bone, ref ev, true);

        Dirty(bone, boneComp);
        return true;
    }

    public bool RandomBoneTraumaChance(WoundableComponent woundableComp, EntityUid woundInflicter, FixedPoint2 severity)
    {
        var wound = Comp<WoundComponent>(woundInflicter);
        var bone = Comp<BoneComponent>(woundableComp.Bone!.ContainedEntities[0]);
        if (woundableComp.WoundableIntegrity <= 0 || bone.BoneIntegrity <= 0)
            return true;

        if (severity < BoneDamageSeverityThreshold)
            return false;

        // We do complete random to get the chance for trauma to happen,
        // We combine multiple parameters and do some math, to get the chance.
        // Even if we get 0.1 damage there's still a chance for injury to be applied, but with the extremely low chance.
        // The more damage, the bigger is the chance.
        var chance = FixedPoint2.Clamp(
            (woundableComp.WoundableIntegrity / (woundableComp.WoundableIntegrity + bone.BoneIntegrity)
            * _boneTraumaChanceMultipliers[woundableComp.WoundableSeverity]) + wound.TraumasChances[TraumaType.BoneDamage],
            0,
            1);

        // Some examples of how this works:
        // 81 / (81 + 20) * 0.1 (Moderate) = 0.08. Or 8%:
        // 57 / (57 + 12) * 0.5 (Severe) = 0.41~. Or 41%;
        // 57 / (57 + 0) * 0.5 (Severe) = 0.5. Or 50%;
        // Yeah lol having your bone already messed up makes the chance of it damaging again higher

        return _random.Prob((float) chance);
    }

    #endregion

    #region Private API

    private void CheckBoneSeverity(EntityUid bone, BoneComponent boneComp)
    {
        var nearestSeverity = boneComp.BoneSeverity;

        foreach (var (severity, value) in _boneThresholds.OrderByDescending(kv => kv.Value))
        {
            if (boneComp.BoneIntegrity < value)
                continue;

            nearestSeverity = severity;
            break;
        }

        if (nearestSeverity != boneComp.BoneSeverity)
        {
            var ev = new BoneSeverityChangedEvent(bone, nearestSeverity);
            RaiseLocalEvent(bone, ref ev, true);

            if (boneComp.BoneWoundable != null)
            {
                var bodyComp = Comp<BodyPartComponent>(boneComp.BoneWoundable.Value);
                if (bodyComp.Body.HasValue)
                {
                    if (boneComp.IntegrityCap / 1.6 > boneComp.BoneIntegrity &&
                        nearestSeverity == BoneSeverity.Damaged)
                    {
                        _audio.PlayPvs(boneComp.BoneBreakSound, bodyComp.Body.Value, AudioParams.Default.WithVolume(-8f));
                    }
                }
            }
        }
        boneComp.BoneSeverity = nearestSeverity;

        Dirty(bone, boneComp);
    }

    private void ApplyBoneDamageEffects(BoneComponent boneComp)
    {
        if (boneComp.BoneWoundable == null)
            return;

        var bodyPart = Comp<BodyPartComponent>(boneComp.BoneWoundable.Value);

        if (bodyPart.Body == null || !TryComp<BodyComponent>(bodyPart.Body, out var body))
            return;

        if (bodyPart.PartType != BodyPartType.Leg || body.RequiredLegs <= 0)
            return;

        if (!TryComp<MovementBodyPartComponent>(boneComp.BoneWoundable, out var movementPart))
            return;

        var modifier = boneComp.BoneSeverity switch
        {
            BoneSeverity.Normal => 1f,
            BoneSeverity.Damaged => 0.6f,
            BoneSeverity.Broken => 0f,
            _ => 1f,
        };

        movementPart.WalkSpeed *= modifier;
        movementPart.SprintSpeed *= modifier;
        movementPart.Acceleration *= modifier;

        UpdateLegsMovementSpeed(bodyPart.Body.Value, body);
    }

    private void ProcessLegsState(EntityUid body, BodyComponent bodyComp)
    {
        var brokenLegs = 0;
        foreach (var legEntity in bodyComp.LegEntities)
        {
            if (!TryComp<WoundableComponent>(legEntity, out var legWoundable))
                continue;

            if (Comp<BoneComponent>(legWoundable.Bone!.ContainedEntities[0]).BoneSeverity == BoneSeverity.Broken)
            {
                brokenLegs++;
            }
        }

        if (brokenLegs >= bodyComp.LegEntities.Count / 2 && brokenLegs < bodyComp.LegEntities.Count)
        {
            _movementSpeed.ChangeBaseSpeed(body, 2.5f * 0.4f, 4.5f * 0.4f, 20f * 0.4f);
        }
        else if (brokenLegs == bodyComp.LegEntities.Count)
        {
            _standing.Down(body);
        }
        else
        {
            _standing.Stand(body);
            _movementSpeed.ChangeBaseSpeed(body, 2.5f, 4.5f, 20f);
        }
    }

    private void UpdateLegsMovementSpeed(EntityUid body, BodyComponent bodyComp)
    {
        var walkSpeed = 0f;
        var sprintSpeed = 0f;
        var acceleration = 0f;

        foreach (var legEntity in bodyComp.LegEntities)
        {
            if (!TryComp<MovementBodyPartComponent>(legEntity, out var legModifier))
                continue;

            if (!TryComp<BodyPartComponent>(legEntity, out var bodyPart))
                continue;

            var feet = _body.GetBodyChildrenOfType(body, BodyPartType.Foot, symmetry: bodyPart.Symmetry).ToList();

            var feetModifier = 1f;
            if (feet.Count != 0 && TryComp<BoneComponent>(feet.First().Id, out var bone) && bone.BoneSeverity == BoneSeverity.Broken)
            {
                feetModifier = 0.2f;
            }

            walkSpeed += legModifier.WalkSpeed * feetModifier;
            sprintSpeed += legModifier.SprintSpeed * feetModifier;
            acceleration += legModifier.Acceleration * feetModifier;
        }

        walkSpeed /= bodyComp.RequiredLegs;
        sprintSpeed /= bodyComp.RequiredLegs;
        acceleration /= bodyComp.RequiredLegs;

        _movementSpeed.ChangeBaseSpeed(body, walkSpeed, sprintSpeed, acceleration);
    }

    #endregion
}
