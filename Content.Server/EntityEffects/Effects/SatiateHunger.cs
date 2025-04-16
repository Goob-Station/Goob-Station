// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects
{
    /// <summary>
    /// Attempts to find a HungerComponent on the target,
    /// and to update it's hunger values.
    /// </summary>
    public sealed partial class SatiateHunger : EntityEffect
    {
        private const float DefaultNutritionFactor = 3.0f;

        /// <summary>
        ///     How much hunger is satiated.
        ///     Is multiplied by quantity if used with EntityEffectReagentArgs.
        /// </summary>
        [DataField("factor")] public float NutritionFactor { get; set; } = DefaultNutritionFactor;

        //Remove reagent at set rate, satiate hunger if a HungerComponent can be found
        public override void Effect(EntityEffectBaseArgs args)
        {
            var entman = args.EntityManager;
            if (!entman.TryGetComponent(args.TargetEntity, out HungerComponent? hunger))
                return;
            if (args is EntityEffectReagentArgs reagentArgs)
            {
                entman.System<HungerSystem>().ModifyHunger(reagentArgs.TargetEntity, NutritionFactor * (float) reagentArgs.Quantity, hunger);
            }
            else
            {
                entman.System<HungerSystem>().ModifyHunger(args.TargetEntity, NutritionFactor, hunger);
            }
        }

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-satiate-hunger", ("chance", Probability), ("relative", NutritionFactor / DefaultNutritionFactor));
    }
}