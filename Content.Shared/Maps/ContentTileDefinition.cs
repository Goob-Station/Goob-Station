// SPDX-FileCopyrightText: 2019 Ephememory <yetanotherscuffed@gmail.com>
// SPDX-FileCopyrightText: 2019 Injazz <43905364+Injazz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2019 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2019 PrPleGoo <felix.leeuwen@gmail.com>
// SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2020 Rohesie <rohesie@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <GalacticChimpanzee@gmail.com>
// SPDX-FileCopyrightText: 2021 Kara D <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Swept <sweptwastaken@protonmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jacob Tong <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@gmail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DoutorWhite <thedoctorwhite@gmail.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Content.Shared.Light.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Tools;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

namespace Content.Shared.Maps
{
    [Prototype("tile")]
    public sealed partial class ContentTileDefinition : IPrototype, IInheritingPrototype, ITileDefinition
    {
        [ValidatePrototypeId<ToolQualityPrototype>]
        public const string PryingToolQuality = "Prying";

        public const string SpaceID = "Space";

        [ParentDataFieldAttribute(typeof(AbstractPrototypeIdArraySerializer<ContentTileDefinition>))]
        public string[]? Parents { get; private set; }

        [NeverPushInheritance]
        [AbstractDataFieldAttribute]
        public bool Abstract { get; private set; }

        [IdDataField] public string ID { get; private set; } = string.Empty;

        public ushort TileId { get; private set; }

        [DataField("name")]
        public string Name { get; private set; } = "";
        [DataField("sprite")] public ResPath? Sprite { get; private set; }

        [DataField("edgeSprites")] public Dictionary<Direction, ResPath> EdgeSprites { get; private set; } = new();

        [DataField("edgeSpritePriority")] public int EdgeSpritePriority { get; private set; } = 0;

        [DataField("isSubfloor")] public bool IsSubFloor { get; private set; }

        [DataField("baseTurf")]
        public string BaseTurf { get; private set; } = string.Empty;

        [DataField]
        public PrototypeFlags<ToolQualityPrototype> DeconstructTools { get; set; } = new();

        /// <summary>
        /// Goobstation
        /// Tile deconstruct do-after time multiplier
        /// </summary>
        [DataField]
        public float DeconstructTimeMultiplier { get; private set; }

        /// <remarks>
        /// Legacy AF but nice to have.
        /// </remarks>
        public bool CanCrowbar => DeconstructTools.Contains(PryingToolQuality);

        /// <summary>
        /// These play when the mob has shoes on.
        /// </summary>
        [DataField("footstepSounds")] public SoundSpecifier? FootstepSounds { get; private set; }

        /// <summary>
        /// These play when the mob has no shoes on.
        /// </summary>
        [DataField("barestepSounds")] public SoundSpecifier? BarestepSounds { get; private set; } = new SoundCollectionSpecifier("BarestepHard");

        [DataField("friction")] public float Friction { get; set; } = 0.2f;

        [DataField("variants")] public byte Variants { get; set; } = 1;

        /// <summary>
        /// This controls what variants the `variantize` command is allowed to use.
        /// </summary>
        [DataField("placementVariants")] public float[] PlacementVariants { get; set; } = { 1f };

        [DataField("thermalConductivity")] public float ThermalConductivity = 0.04f;

        // Heat capacity is opt-in, not opt-out.
        [DataField("heatCapacity")] public float HeatCapacity = Atmospherics.MinimumHeatCapacity;

        [DataField("itemDrop", customTypeSerializer:typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string ItemDropPrototypeName { get; private set; } = "FloorTileItemSteel";

        // TODO rename data-field in yaml
        /// <summary>
        /// Whether or not the tile is exposed to the map's atmosphere.
        /// </summary>
        [DataField("isSpace")] public bool MapAtmosphere { get; private set; }

        /// <summary>
        ///     Friction override for mob mover in <see cref="SharedMoverController"/>
        /// </summary>
        [DataField("mobFriction")]
        public float? MobFriction { get; private set; }

        /// <summary>
        ///     No-input friction override for mob mover in <see cref="SharedMoverController"/>
        /// </summary>
        [DataField("mobFrictionNoInput")]
        public float? MobFrictionNoInput { get; private set; }

        /// <summary>
        ///     Accel override for mob mover in <see cref="SharedMoverController"/>
        /// </summary>
        [DataField("mobAcceleration")]
        public float? MobAcceleration { get; private set; }

        [DataField("sturdy")] public bool Sturdy { get; private set; } = true;

        /// <summary>
        /// Can weather affect this tile.
        /// </summary>
        [DataField("weather")] public bool Weather = false;

        /// <summary>
        /// Is this tile immune to RCD deconstruct.
        /// </summary>
        [DataField("indestructible")] public bool Indestructible = false;

        public void AssignTileId(ushort id)
        {
            TileId = id;
        }

        [DataField]
        public bool Reinforced = false;

        [DataField]
        public float TileRipResistance = 125f;
    }
}