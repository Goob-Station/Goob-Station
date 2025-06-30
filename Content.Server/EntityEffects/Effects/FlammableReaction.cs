// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Database;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects
{
    [UsedImplicitly]
    public sealed partial class FlammableReaction : EntityEffect
    {
        [DataField]
        public float Multiplier = 0.05f;

        // The fire stack multiplier if fire stacks already exist on target, only works if 0 or greater
        [DataField]
        public float MultiplierOnExisting = -1f;

        public override bool ShouldLog => true;

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-flammable-reaction", ("chance", Probability));

        public override LogImpact LogImpact => LogImpact.Medium;

        public override void Effect(EntityEffectBaseArgs args)
        {
            if (!args.EntityManager.TryGetComponent(args.TargetEntity, out FlammableComponent? flammable))
                return;

            // Sets the multiplier for FireStacks to MultiplierOnExisting is 0 or greater and target already has FireStacks
            var multiplier = flammable.FireStacks != 0f && MultiplierOnExisting >= 0 ? MultiplierOnExisting : Multiplier;
            var quantity = 1f;
            if (args is EntityEffectReagentArgs reagentArgs)
            {
                quantity = reagentArgs.Quantity.Float();
                reagentArgs.EntityManager.System<FlammableSystem>().AdjustFireStacks(args.TargetEntity, quantity * multiplier, flammable);
                if (reagentArgs.Reagent != null)
                    reagentArgs.Source?.RemoveReagent(reagentArgs.Reagent.ID, reagentArgs.Quantity);
            }
            else
            {
                args.EntityManager.System<FlammableSystem>().AdjustFireStacks(args.TargetEntity, multiplier, flammable);
            }
        }
    }
}