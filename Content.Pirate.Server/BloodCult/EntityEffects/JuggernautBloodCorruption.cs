// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using Content.Server.Fluids.EntitySystems;
using Content.Shared.BloodCult;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
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

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-juggernaut-blood-corruption", ("chance", Probability));

    public override void RaiseEvent(EntityUid target, IEntityEffectRaiser raiser, float scale, EntityUid? user)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();

        if (!LegacyEntityEffectContext.TryGetReaction(entMan, out var reaction) ||
            reaction.Reagent.ID == CorruptedReagent ||
            !BloodCultConstants.SacrificeBloodReagents.Contains(reaction.Reagent.ID) ||
            reaction.ReagentQuantity.Quantity <= 0)
        {
            return;
        }

        var puddleSystem = entMan.System<PuddleSystem>();
        var transform = entMan.GetComponent<TransformComponent>(target);

        // Create a solution of Sanguine Perniculate with the same volume as the reagent quantity that was applied
        var corruptedSolution = new Solution();
        corruptedSolution.AddReagent(CorruptedReagent, reaction.ReagentQuantity.Quantity);

        // Spawn a puddle at the juggernaut's feet
        puddleSystem.TrySpillAt(transform.Coordinates, corruptedSolution, out _, sound: false);
    }
}
