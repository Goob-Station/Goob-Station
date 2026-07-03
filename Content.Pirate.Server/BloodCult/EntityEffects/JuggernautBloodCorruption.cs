// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using Content.Server.Fluids.EntitySystems;
using Content.Shared.BloodCult;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server.BloodCult.EntityEffects;

/// <summary>
/// When blood is splashed on a juggernaut, creates Sanguine Perniculate puddles on the ground.
/// This represents the blood being corrupted by the construct's unholy essence.
/// </summary>
public sealed partial class JuggernautBloodCorruption : EntityEffect
{
    [DataField]
    public ProtoId<ReagentPrototype> CorruptedReagent = "SanguinePerniculate";

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-juggernaut-blood-corruption", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        // Only process if we have reagent args
        if (args is not EntityEffectReagentArgs reagentArgs)
            return;

        // Validate the reagent being applied
        var reagentId = reagentArgs.Reagent?.ID;
        if (string.IsNullOrEmpty(reagentId))
            return;

        // Ignore already corrupted blood or reagents that shouldn't be corrupted.
        if (reagentId == CorruptedReagent || !BloodCultConstants.SacrificeBloodReagents.Contains(reagentId))
            return;

        // No quantity, no corruption.
        if (reagentArgs.Quantity <= FixedPoint2.Zero)
            return;

        var puddleSystem = args.EntityManager.System<PuddleSystem>();
        var transform = args.EntityManager.GetComponent<TransformComponent>(args.TargetEntity);

        // Create a solution of Sanguine Perniculate with the same volume as the reagent quantity that was applied
        var corruptedSolution = new Solution();
        corruptedSolution.AddReagent(CorruptedReagent, reagentArgs.Quantity);

        // Spawn a puddle at the juggernaut's feet
        puddleSystem.TrySpillAt(transform.Coordinates, corruptedSolution, out _, sound: false);
    }
}

