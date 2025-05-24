// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects
{
    [UsedImplicitly]
    public sealed partial class ExtinguishReaction : EntityEffect
    {
        /// <summary>
        ///     Amount of firestacks reduced.
        /// </summary>
        [DataField]
        public float FireStacksAdjustment = -1.5f;

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-extinguish-reaction", ("chance", Probability));

        public override void Effect(EntityEffectBaseArgs args)
        {
            var ev = new ExtinguishEvent
            {
                FireStacksAdjustment = FireStacksAdjustment,
            };

            if (args is EntityEffectReagentArgs reagentArgs)
            {
                ev.FireStacksAdjustment *= (float)reagentArgs.Quantity;
            }

            args.EntityManager.EventBus.RaiseLocalEvent(args.TargetEntity, ref ev);
        }
    }
}