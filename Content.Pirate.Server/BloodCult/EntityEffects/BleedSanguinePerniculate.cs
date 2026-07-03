// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;
using Content.Shared.Chemistry.Reagent;

namespace Content.Server.BloodCult.EntityEffects;

/// <summary>
/// Makes an entity bleed Sanguine Perniculate instead of their normal blood type while they metabolize Edge Essentia.
/// Changes what blood they bleed out, not their internal blood.
/// </summary>
public sealed partial class BleedSanguinePerniculate : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-bleed-sanguine-perniculate", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        // Verify target entity exists (could be deleted during metabolism processing)
        if (!args.EntityManager.EntityExists(args.TargetEntity))
            return;

        if (!args.EntityManager.TryGetComponent<BloodstreamComponent>(args.TargetEntity, out var bloodstream))
            return;

        // Store their original blood type if not already stored
        if (!args.EntityManager.TryGetComponent<EdgeEssentiaBloodComponent>(args.TargetEntity, out var edgeEssentiaComp))
        {
            edgeEssentiaComp = args.EntityManager.AddComponent<EdgeEssentiaBloodComponent>(args.TargetEntity);
            if (!TryGetPrototypeBloodReagent(args.TargetEntity, args.EntityManager, out var originalBlood))
                originalBlood = bloodstream.BloodReagent;

            edgeEssentiaComp.OriginalBloodReagent = originalBlood;
        }

        // Change their blood type to Sanguine Perniculate so that when they bleed, it comes out as Sanguine Perniculate
        // This happens every metabolism tick, ensuring their blood type stays as SanguinePerniculate while Edge Essentia is active
        var bloodstreamSystem = args.EntityManager.System<BloodstreamSystem>();
        bloodstreamSystem.ChangeBloodReagent(args.TargetEntity, "SanguinePerniculate", bloodstream);
    }

    private bool TryGetPrototypeBloodReagent(EntityUid uid, IEntityManager entityManager, out ProtoId<ReagentPrototype> bloodReagent)
    {
        bloodReagent = default!;

        if (!entityManager.TryGetComponent<MetaDataComponent>(uid, out var meta) || meta.EntityPrototype == null)
            return false;

        var componentFactory = entityManager.ComponentFactory;
        if (!meta.EntityPrototype.TryGetComponent(componentFactory.GetComponentName<BloodstreamComponent>(), out BloodstreamComponent? prototypeBloodstream))
            return false;

        bloodReagent = prototypeBloodstream.BloodReagent;
        return true;
    }
}

/// <summary>
/// Component to track the original blood type of an entity affected by Edge Essentia
/// and how much Sanguine Perniculate they've bled for the ritual pool.
/// </summary>
[RegisterComponent]
public sealed partial class EdgeEssentiaBloodComponent : Component
{
    /// <summary>
    /// The original blood reagent before Edge Essentia changed it
    /// </summary>
    [DataField]
    public string OriginalBloodReagent = "Blood";

    /// <summary>
    /// Tracks the last amount of Sanguine Perniculate in the temporary solution to detect new bleeding
    /// </summary>
    [DataField]
    public FixedPoint2 LastTrackedBloodAmount = FixedPoint2.Zero;
}

