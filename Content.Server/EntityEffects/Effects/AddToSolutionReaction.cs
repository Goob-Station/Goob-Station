// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects
{
    [UsedImplicitly]
    public sealed partial class AddToSolutionReaction : EntityEffect
    {
        [DataField("solution")]
        private string _solution = "reagents";

        public override void Effect(EntityEffectBaseArgs args)
        {
            if (args is EntityEffectReagentArgs reagentArgs) {
                if (reagentArgs.Reagent == null)
                    return;

                // TODO see if this is correct
                var solutionContainerSystem = reagentArgs.EntityManager.System<SharedSolutionContainerSystem>();
                if (!solutionContainerSystem.TryGetSolution(reagentArgs.TargetEntity, _solution, out var solutionContainer))
                    return;

                if (solutionContainerSystem.TryAddReagent(solutionContainer.Value, reagentArgs.Reagent.ID, reagentArgs.Quantity, out var accepted))
                    reagentArgs.Source?.RemoveReagent(reagentArgs.Reagent.ID, accepted);

                return;
            }

            // TODO: Someone needs to figure out how to do this for non-reagent effects.
            throw new NotImplementedException();
        }

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
            Loc.GetString("reagent-effect-guidebook-add-to-solution-reaction", ("chance", Probability));
    }
}