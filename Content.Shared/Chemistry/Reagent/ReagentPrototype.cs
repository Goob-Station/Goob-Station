// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2019 moneyl <8206401+Moneyl@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 nuke <47336974+nuke-makes-games@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Injazz <injazza@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 py01 <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 corentt <36075110+corentt@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Sam Weaver <weaversam8@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Flesh <62557990+PolterTzi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Adrian16199 <144424013+Adrian16199@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Collections.Frozen;
using System.Linq;
using System.Text.Json.Serialization;
using Content.Shared.Administration.Logs;
using Content.Shared.Body.Prototypes;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.EntityEffects;
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.Nutrition;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

namespace Content.Shared.Chemistry.Reagent
{
    [Prototype("reagent")]
    [DataDefinition]
    public sealed partial class ReagentPrototype : IPrototype, IInheritingPrototype
    {
        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField(required: true)]
        private LocId Name { get; set; }

        [ViewVariables(VVAccess.ReadOnly)]
        public string LocalizedName => Loc.GetString(Name);

        [DataField]
        public string Group { get; private set; } = "Unknown";

        [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<ReagentPrototype>))]
        public string[]? Parents { get; private set; }

        [NeverPushInheritance]
        [AbstractDataField]
        public bool Abstract { get; private set; }

        [DataField("desc", required: true)]
        private LocId Description { get; set; }

        [ViewVariables(VVAccess.ReadOnly)]
        public string LocalizedDescription => Loc.GetString(Description);

        [DataField("physicalDesc", required: true)]
        private LocId PhysicalDescription { get; set; } = default!;

        [ViewVariables(VVAccess.ReadOnly)]
        public string LocalizedPhysicalDescription => Loc.GetString(PhysicalDescription);

        /// <summary>
        ///     Is this reagent recognizable to the average spaceman (water, welding fuel, ketchup, etc)?
        /// </summary>
        [DataField]
        public bool Recognizable;

        [DataField]
        public ProtoId<FlavorPrototype>? Flavor;

        /// <summary>
        /// There must be at least this much quantity in a solution to be tasted.
        /// </summary>
        [DataField]
        public FixedPoint2 FlavorMinimum = FixedPoint2.New(0.1f);

        [DataField("color")]
        public Color SubstanceColor { get; private set; } = Color.White;

        /// <summary>
        ///     The specific heat of the reagent.
        ///     How much energy it takes to heat one unit of this reagent by one Kelvin.
        /// </summary>
        [DataField]
        public float SpecificHeat { get; private set; } = 1.0f;

        [DataField]
        public float? BoilingPoint { get; private set; }

        [DataField]
        public float? MeltingPoint { get; private set; }

        [DataField]
        public SpriteSpecifier? MetamorphicSprite { get; private set; } = null;

        [DataField]
        public int MetamorphicMaxFillLevels { get; private set; } = 0;

        [DataField]
        public string? MetamorphicFillBaseName { get; private set; } = null;

        [DataField]
        public bool MetamorphicChangeColor { get; private set; } = true;

        /// <summary>
        /// If this reagent is part of a puddle is it slippery.
        /// </summary>
        [DataField]
        public bool Slippery;

        /// <summary>
        /// How easily this reagent becomes fizzy when aggitated.
        /// 0 - completely flat, 1 - fizzes up when nudged.
        /// </summary>
        [DataField]
        public float Fizziness;

        /// <summary>
        /// How much reagent slows entities down if it's part of a puddle.
        /// 0 - no slowdown; 1 - can't move.
        /// </summary>
        [DataField]
        public float Viscosity;

        /// <summary>
        /// Should this reagent work on the dead?
        /// </summary>
        [DataField]
        public bool WorksOnTheDead;

        /// <summary>
        /// Should this reagent only work on unconscious entities?
        /// </summary>
        [DataField]
        public bool? WorksOnUnconscious;

        [DataField(serverOnly: true)]
        public FrozenDictionary<ProtoId<MetabolismGroupPrototype>, ReagentEffectsEntry>? Metabolisms;

        [DataField(serverOnly: true)]
        public Dictionary<ProtoId<ReactiveGroupPrototype>, ReactiveReagentEffectEntry>? ReactiveEffects;

        [DataField(serverOnly: true)]
        public List<ITileReaction> TileReactions = new(0);

        [DataField("plantMetabolism", serverOnly: true)]
        public List<EntityEffect> PlantMetabolisms = new(0);

        [DataField]
        public float PricePerUnit;

        [DataField]
        public SoundSpecifier FootstepSound = new SoundCollectionSpecifier("FootstepWater", AudioParams.Default.WithVolume(6));

        public FixedPoint2 ReactionTile(TileRef tile, FixedPoint2 reactVolume, IEntityManager entityManager, List<ReagentData>? data)
        {
            var removed = FixedPoint2.Zero;

            if (tile.Tile.IsEmpty)
                return removed;

            foreach (var reaction in TileReactions)
            {
                removed += reaction.TileReact(tile, this, reactVolume - removed, entityManager, data);

                if (removed > reactVolume)
                    throw new Exception("Removed more than we have!");

                if (removed == reactVolume)
                    break;
            }

            return removed;
        }

        public void ReactionPlant(EntityUid? plantHolder, ReagentQuantity amount, Solution solution)
        {
            if (plantHolder == null)
                return;

            var entMan = IoCManager.Resolve<IEntityManager>();
            var random = IoCManager.Resolve<IRobustRandom>();
            var args = new EntityEffectReagentArgs(plantHolder.Value, entMan, null, solution, amount.Quantity, this, null, 1f);
            foreach (var plantMetabolizable in PlantMetabolisms)
            {
                if (!plantMetabolizable.ShouldApply(args, random))
                    continue;

                if (plantMetabolizable.ShouldLog)
                {
                    var entity = args.TargetEntity;
                    entMan.System<SharedAdminLogSystem>().Add(LogType.ReagentEffect, plantMetabolizable.LogImpact,
                        $"Plant metabolism effect {plantMetabolizable.GetType().Name:effect} of reagent {ID:reagent} applied on entity {entMan.ToPrettyString(entity):entity} at {entMan.GetComponent<TransformComponent>(entity).Coordinates:coordinates}");
                }

                plantMetabolizable.Effect(args);
            }
        }
    }

    [Serializable, NetSerializable]
    public struct ReagentGuideEntry
    {
        public string ReagentPrototype;

        public Dictionary<ProtoId<MetabolismGroupPrototype>, ReagentEffectsGuideEntry>? GuideEntries;

        public List<string>? PlantMetabolisms = null;

        public ReagentGuideEntry(ReagentPrototype proto, IPrototypeManager prototype, IEntitySystemManager entSys)
        {
            ReagentPrototype = proto.ID;
            GuideEntries = proto.Metabolisms?
                .Select(x => (x.Key, x.Value.MakeGuideEntry(prototype, entSys)))
                .ToDictionary(x => x.Key, x => x.Item2);
            if (proto.PlantMetabolisms.Count > 0)
            {
                PlantMetabolisms = new List<string> (proto.PlantMetabolisms
                    .Select(x => x.GuidebookEffectDescription(prototype, entSys))
                    .Where(x => x is not null)
                    .Select(x => x!)
                    .ToArray());
            }
        }
    }


    [DataDefinition]
    public sealed partial class ReagentEffectsEntry
    {
        /// <summary>
        ///     Amount of reagent to metabolize, per metabolism cycle.
        /// </summary>
        [JsonPropertyName("rate")]
        [DataField("metabolismRate")]
        public FixedPoint2 MetabolismRate = FixedPoint2.New(0.5f);

        /// <summary>
        ///     A list of effects to apply when these reagents are metabolized.
        /// </summary>
        [JsonPropertyName("effects")]
        [DataField("effects", required: true)]
        public EntityEffect[] Effects = default!;

        public ReagentEffectsGuideEntry MakeGuideEntry(IPrototypeManager prototype, IEntitySystemManager entSys)
        {
            return new ReagentEffectsGuideEntry(MetabolismRate,
                Effects
                    .Select(x => x.GuidebookEffectDescription(prototype, entSys)) // hate.
                    .Where(x => x is not null)
                    .Select(x => x!)
                    .ToArray());
        }
    }

    [Serializable, NetSerializable]
    public struct ReagentEffectsGuideEntry
    {
        public FixedPoint2 MetabolismRate;

        public string[] EffectDescriptions;

        public ReagentEffectsGuideEntry(FixedPoint2 metabolismRate, string[] effectDescriptions)
        {
            MetabolismRate = metabolismRate;
            EffectDescriptions = effectDescriptions;
        }
    }

    [DataDefinition]
    public sealed partial class ReactiveReagentEffectEntry
    {
        [DataField("methods", required: true)]
        public HashSet<ReactionMethod> Methods = default!;

        [DataField("effects", required: true)]
        public EntityEffect[] Effects = default!;
    }
}