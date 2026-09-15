// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Oskarrr.Surgery;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared._Oskarrr.EntityEffects;

/// <summary>
/// Regenerates missing bones on boneless body parts (one part per metabolism tick by default).
/// </summary>
public sealed partial class RegenerateBones : EntityEffectBase<RegenerateBones>
{
    /// <summary>
    /// How many boneless parts to restore per application.
    /// </summary>
    [DataField]
    public int PartsPerTick = 1;

    /// <summary>
    /// Optional integrity to set on newly grown bones. Null keeps prototype default.
    /// </summary>
    [DataField]
    public FixedPoint2? StartingIntegrity;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-regenerate-bones");
}

public sealed class RegenerateBonesEffectSystem : EntityEffectSystem<BodyComponent, RegenerateBones>
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly BoneHarvestSurgerySystem _boneHarvest = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    protected override void Effect(Entity<BodyComponent> ent, ref EntityEffectEvent<RegenerateBones> args)
    {
        var boneless = _body.GetBodyChildrenWithComponent<WoundableComponent>(ent.Owner, ent.Comp)
            .Where(p => HasComp<BonesHarvestedComponent>(p.Id))
            .ToList();

        if (boneless.Count == 0)
            return;

        _random.Shuffle(boneless);

        var count = Math.Min(args.Effect.PartsPerTick, boneless.Count);
        for (var i = 0; i < count; i++)
        {
            var (part, _, woundable) = boneless[i];
            if (!_boneHarvest.RegrowBonesOnPart(part, woundable))
                continue;

            if (args.Effect.StartingIntegrity is not { } integrity)
                continue;

            if (!TryComp(part, out WoundableComponent? updated) ||
                updated.Bone.ContainedEntities.FirstOrNull() is not { } bone)
                continue;

            _trauma.SetBoneIntegrity(bone, integrity);
        }
    }
}
