using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas;

public readonly struct TraumaChanceArgs
{
    public readonly IEntityManager EntityManager;
    public readonly Entity<WoundableComponent> Target;
    public readonly Entity<TraumaInflicterComponent> Inflicter;
    public readonly FixedPoint2 Severity;
    public readonly BodyPartComponent Part;
    public readonly EntityUid Body;

    public TraumaChanceArgs(
        IEntityManager entityManager,
        Entity<WoundableComponent> target,
        Entity<TraumaInflicterComponent> inflicter,
        FixedPoint2 severity,
        BodyPartComponent part,
        EntityUid body)
    {
        EntityManager = entityManager;
        Target = target;
        Inflicter = inflicter;
        Severity = severity;
        Part = part;
        Body = body;
    }
}

[ImplicitDataDefinitionForInheritors]
public abstract partial class TraumaChance
{
    public abstract FixedPoint2? Calculate(in TraumaChanceArgs args);
}

public sealed partial class FlatTraumaChance : TraumaChance
{
    [DataField]
    public FixedPoint2 Chance;

    public override FixedPoint2? Calculate(in TraumaChanceArgs args)
    {
        return Chance;
    }
}

/// <summary>
/// Bone fracture chance, scales with how damaged the part and its bone already are.
/// </summary>
public sealed partial class BoneFractureChance : TraumaChance
{
    [DataField]
    public Dictionary<WoundableSeverity, FixedPoint2> SeverityMultipliers = new()
    {
        { WoundableSeverity.Healthy, 0 },
        { WoundableSeverity.Minor, 0.01 },
        { WoundableSeverity.Moderate, 0.04 },
        { WoundableSeverity.Severe, 0.12 },
        { WoundableSeverity.Critical, 0.21 },
        { WoundableSeverity.Mangled, 0.21 },
        { WoundableSeverity.Severed, 0 },
    };

    public override FixedPoint2? Calculate(in TraumaChanceArgs args)
    {
        var target = args.Target;
        if (target.Comp.Bone.ContainedEntities.FirstOrNull() is not { } bone
            || !args.EntityManager.TryGetComponent(bone, out BoneComponent? boneComp)
            || boneComp.BoneSeverity == BoneSeverity.Broken)
            return null;

        return target.Comp.IntegrityCap / (target.Comp.WoundableIntegrity + boneComp.BoneIntegrity)
            * SeverityMultipliers.GetValueOrDefault(target.Comp.WoundableSeverity);
    }
}

/// <summary>
/// Nerve damage chance, only on parts that can still feel pain.
/// </summary>
public sealed partial class NerveTraumaChance : TraumaChance
{
    [DataField]
    public float MinPainFeels = 0.2f;

    [DataField]
    public FixedPoint2 Divisor = 20;

    public override FixedPoint2? Calculate(in TraumaChanceArgs args)
    {
        if (!args.EntityManager.TryGetComponent(args.Target, out NerveComponent? nerve)
            || nerve.PainFeels < MinPainFeels)
            return null;

        return args.Target.Comp.WoundableIntegrity / args.Target.Comp.IntegrityCap / Divisor;
    }
}

/// <summary>
/// Organ damage chance, dealt to a random surviving organ in the part.
/// </summary>
public sealed partial class OrganTraumaChance : TraumaChance
{
    [DataField]
    public FixedPoint2 BaseChance = 0.4;

    public override FixedPoint2? Calculate(in TraumaChanceArgs args)
    {
        var body = args.EntityManager.System<SharedBodySystem>();
        var hasOrgan = false;
        foreach (var organ in body.GetPartOrgans(args.Target, args.Part))
        {
            if (organ.Component.OrganIntegrity <= 0)
                continue;

            hasOrgan = true;
            break;
        }

        return hasOrgan ? BaseChance : null;
    }
}

/// <summary>
/// Dismemberment chance, scales with how damaged the part and its bone already are.
/// </summary>
public sealed partial class DismembermentChance : TraumaChance
{
    [DataField]
    public float IntegrityExponent = 1.3f;

    [DataField]
    public Dictionary<BoneSeverity, float> BoneMultipliers = new()
    {
        { BoneSeverity.Normal, 0.3f },
        { BoneSeverity.Damaged, 0.6f },
        { BoneSeverity.Cracked, 1f },
        { BoneSeverity.Broken, 1.2f },
    };

    [DataField]
    public Dictionary<ProtoId<DamageTypePrototype>, float> DamageTypeMultipliers = new();

    public override FixedPoint2? Calculate(in TraumaChanceArgs args)
    {
        var em = args.EntityManager;
        var target = args.Target;

        if (target.Comp.ParentWoundable is not { } parentWoundable)
            return null;

        if (args.Part.PartType == BodyPartType.Chest
            || args.Part.PartType == BodyPartType.Groin
                && em.GetComponent<WoundableComponent>(parentWoundable).WoundableSeverity != WoundableSeverity.Mangled)
            return null;

        var bonePenalty = FixedPoint2.New(1);
        if (em.TryGetComponent(target, out BonelessComponent? bonelessComp))
            bonePenalty = bonelessComp.BonePenalty;

        var multiplier = 1f;
        if (target.Comp.Bone.ContainedEntities.FirstOrNull() is { } bone
            && em.TryGetComponent(bone, out BoneComponent? boneComp))
            multiplier = BoneMultipliers.GetValueOrDefault(boneComp.BoneSeverity, 1f);

        if (DamageTypeMultipliers.Count > 0
            && em.TryGetComponent(args.Inflicter, out WoundComponent? inflicterWound))
            multiplier *= DamageTypeMultipliers.GetValueOrDefault(inflicterWound.DamageType, 1f);

        return (1f - (MathF.Pow(target.Comp.WoundableIntegrity.Float(), IntegrityExponent) / target.Comp.IntegrityCap - 1f) * bonePenalty) * multiplier;
    }
}
