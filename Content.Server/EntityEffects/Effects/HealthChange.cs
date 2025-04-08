// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2021 ColdAutumnRain <73938872+ColdAutumnRain@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Fortune117 <fortune11709@gmail.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <GalacticChimpanzee@gmail.com>
// SPDX-FileCopyrightText: 2021 Jaskanbe <86671825+Jaskanbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Kara Dinyes <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+leonsfriedrich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2021 Michael Will <will_m@outlook.de>
// SPDX-FileCopyrightText: 2021 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 SETh lafuente <cetaciocascarudo@gmail.com>
// SPDX-FileCopyrightText: 2021 ScalyChimp <72841710+scaly-chimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 SethLafuente <84478872+SethLafuente@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2021 Silver <silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
// SPDX-FileCopyrightText: 2021 TimrodDX <timrod@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <notzombiedude@gmail.com>
// SPDX-FileCopyrightText: 2021 scrato <Mickaello2003@gmx.de>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Sam Weaver <weaversam8@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2023 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 KrasnoshchekovPavel <119816022+KrasnoshchekovPavel@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Content.Shared.Localizations;
using Content.Shared._Shitmed.EntityEffects.Effects; // Shitmed Change
using Content.Shared._Shitmed.Targeting; // Shitmed Change
using Content.Server.Temperature.Components; // Shitmed Change
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using System.Linq;
using System.Text.Json.Serialization;

namespace Content.Server.EntityEffects.Effects
{
    /// <summary>
    /// Default metabolism used for medicine reagents.
    /// </summary>
    [UsedImplicitly]
    public sealed partial class HealthChange : EntityEffect
    {
        /// <summary>
        /// Damage to apply every cycle. Damage Ignores resistances.
        /// </summary>
        [DataField(required: true)]
        [JsonPropertyName("damage")]
        public DamageSpecifier Damage = default!;

        /// <summary>
        ///     Should this effect scale the damage by the amount of chemical in the solution?
        ///     Useful for touch reactions, like styptic powder or acid.
        ///     Only usable if the EntityEffectBaseArgs is an EntityEffectReagentArgs.
        /// </summary>
        [DataField]
        [JsonPropertyName("scaleByQuantity")]
        public bool ScaleByQuantity;

        /// <summary>
        ///     Scales the effect based on the temperature of the entity.
        /// </summary>
        [DataField]
        [JsonPropertyName("scaleByTemperature")]
        public TemperatureScaling? ScaleByTemperature;

        [DataField]
        [JsonPropertyName("ignoreResistances")]
        public bool IgnoreResistances = true;

        protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        {
            var damages = new List<string>();
            var heals = false;
            var deals = false;

            var damageSpec = new DamageSpecifier(Damage);

            var universalReagentDamageModifier =
                entSys.GetEntitySystem<DamageableSystem>().UniversalReagentDamageModifier;
            var universalReagentHealModifier = entSys.GetEntitySystem<DamageableSystem>().UniversalReagentHealModifier;

            if (universalReagentDamageModifier != 1 || universalReagentHealModifier != 1)
            {
                foreach (var (type, val) in damageSpec.DamageDict)
                {
                    if (val < 0f)
                    {
                        damageSpec.DamageDict[type] = val * universalReagentHealModifier;
                    }

                    if (val > 0f)
                    {
                        damageSpec.DamageDict[type] = val * universalReagentDamageModifier;
                    }
                }
            }

            damageSpec = entSys.GetEntitySystem<DamageableSystem>().ApplyUniversalAllModifiers(damageSpec);

            foreach (var group in prototype.EnumeratePrototypes<DamageGroupPrototype>())
            {
                if (!damageSpec.TryGetDamageInGroup(group, out var amount))
                    continue;

                var relevantTypes = damageSpec.DamageDict
                    .Where(x => x.Value != FixedPoint2.Zero && group.DamageTypes.Contains(x.Key))
                    .ToList();

                if (relevantTypes.Count != group.DamageTypes.Count)
                    continue;

                var sum = FixedPoint2.Zero;
                foreach (var type in group.DamageTypes)
                {
                    sum += damageSpec.DamageDict.GetValueOrDefault(type);
                }

                // if the total sum of all the types equal the damage amount,
                // assume that they're evenly distributed.
                if (sum != amount)
                    continue;

                var sign = FixedPoint2.Sign(amount);

                if (sign < 0)
                    heals = true;
                if (sign > 0)
                    deals = true;

                damages.Add(
                    Loc.GetString("health-change-display",
                        ("kind", group.LocalizedName),
                        ("amount", MathF.Abs(amount.Float())),
                        ("deltasign", sign)
                    ));

                foreach (var type in group.DamageTypes)
                {
                    damageSpec.DamageDict.Remove(type);
                }
            }

            foreach (var (kind, amount) in damageSpec.DamageDict)
            {
                var sign = FixedPoint2.Sign(amount);

                if (sign < 0)
                    heals = true;
                if (sign > 0)
                    deals = true;

                damages.Add(
                    Loc.GetString("health-change-display",
                        ("kind", prototype.Index<DamageTypePrototype>(kind).LocalizedName),
                        ("amount", MathF.Abs(amount.Float())),
                        ("deltasign", sign)
                    ));
            }

            var healsordeals = heals ? (deals ? "both" : "heals") : (deals ? "deals" : "none");

            return Loc.GetString("reagent-effect-guidebook-health-change",
                ("chance", Probability),
                ("changes", ContentLocalizationManager.FormatList(damages)),
                ("healsordeals", healsordeals));
        }

        public override void Effect(EntityEffectBaseArgs args)
        {
            var scale = FixedPoint2.New(1);
            var damageSpec = new DamageSpecifier(Damage);

            if (args is EntityEffectReagentArgs reagentArgs)
            {
                scale = ScaleByQuantity ? reagentArgs.Quantity * reagentArgs.Scale : reagentArgs.Scale;
            }

            if (ScaleByTemperature.HasValue)
            {
                if (!args.EntityManager.TryGetComponent<TemperatureComponent>(args.TargetEntity, out var temp))
                    scale = FixedPoint2.Zero;
                else
                    scale *= ScaleByTemperature.Value.GetEfficiencyMultiplier(temp.CurrentTemperature, scale, false);
            }

            var universalReagentDamageModifier =
                args.EntityManager.System<DamageableSystem>().UniversalReagentDamageModifier;
            var universalReagentHealModifier =
                args.EntityManager.System<DamageableSystem>().UniversalReagentHealModifier;

            if (Math.Abs(universalReagentDamageModifier - 1) > 1 || Math.Abs(universalReagentHealModifier - 1) > 1)
            {
                foreach (var (type, val) in damageSpec.DamageDict)
                {
                    if (val < 0f)
                    {
                        damageSpec.DamageDict[type] = val * universalReagentHealModifier;
                    }

                    if (val > 0f)
                    {
                        damageSpec.DamageDict[type] = val * universalReagentDamageModifier;
                    }
                }
            }

            args.EntityManager.System<DamageableSystem>()
                .TryChangeDamage(
                    args.TargetEntity,
                    damageSpec * scale,
                    IgnoreResistances,
                    interruptsDoAfters: false,
                    // Shitmed Change Start
                    targetPart: TargetBodyPart.All,
                    partMultiplier: 0.5f,
                    canSever: false);
            // Shitmed Change End

        }
    }
}