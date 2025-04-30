// SPDX-FileCopyrightText: 2018 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2019 Silver <Silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2020 Exp <theexp111@gmail.com>
// SPDX-FileCopyrightText: 2020 Swept <jamesurquhartwebb@gmail.com>
// SPDX-FileCopyrightText: 2020 Vera Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 py01 <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 py01 <pyronetics01@gmail.com>
// SPDX-FileCopyrightText: 2020 zumorica <zddm@outlook.es>
// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2022 Kara D <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 08A <git@08a.re>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 PixelTK <85175107+PixelTheKermit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Hrosts <35345601+Hrosts@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Construction.Conditions;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Utility;

namespace Content.Shared.Construction.Prototypes;

[Prototype("construction")]
public sealed partial class ConstructionPrototype : IPrototype
{
    [DataField("conditions")] private List<IConstructionCondition> _conditions = new();

    /// <summary>
    ///     Hide from the construction list
    /// </summary>
    [DataField("hide")]
    public bool Hide = false;

    /// <summary>
    ///     Friendly name displayed in the construction GUI.
    /// </summary>
    [DataField("name")]
    public string Name = string.Empty;

    /// <summary>
    ///     "Useful" description displayed in the construction GUI.
    /// </summary>
    [DataField("description")]
    public string Description = string.Empty;

    /// <summary>
    ///     The <see cref="ConstructionGraphPrototype"/> this construction will be using.
    /// </summary>
    [DataField("graph", customTypeSerializer: typeof(PrototypeIdSerializer<ConstructionGraphPrototype>), required: true)]
    public string Graph = string.Empty;

    /// <summary>
    ///     The target <see cref="ConstructionGraphNode"/> this construction will guide the user to.
    /// </summary>
    [DataField("targetNode")]
    public string TargetNode = string.Empty;

    /// <summary>
    ///     The starting <see cref="ConstructionGraphNode"/> this construction will start at.
    /// </summary>
    [DataField("startNode")]
    public string StartNode = string.Empty;

    /// <summary>
    ///     Texture path inside the construction GUI.
    /// </summary>
    [DataField("icon")]
    public SpriteSpecifier Icon = SpriteSpecifier.Invalid;

    /// <summary>
    ///     Texture paths used for the construction ghost.
    /// </summary>
    [DataField("layers")]
    private List<SpriteSpecifier>? _layers;

    /// <summary>
    ///     If you can start building or complete steps on impassable terrain.
    /// </summary>
    [DataField("canBuildInImpassable")]
    public bool CanBuildInImpassable { get; private set; }

    /// <summary>
    /// If not null, then this is used to check if the entity trying to construct this is whitelisted.
    /// If they're not whitelisted, hide the item.
    /// </summary>
    [DataField("entityWhitelist")]
    public EntityWhitelist? EntityWhitelist = null;

    [DataField("category")] public string Category { get; private set; } = "";

    [DataField("objectType")] public ConstructionType Type { get; private set; } = ConstructionType.Structure;

    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("placementMode")]
    public string PlacementMode = "PlaceFree";

    /// <summary>
    ///     Whether this construction can be constructed rotated or not.
    /// </summary>
    [DataField("canRotate")]
    public bool CanRotate = true;

    /// <summary>
    ///     Construction to replace this construction with when the current one is 'flipped'
    /// </summary>
    [DataField("mirror", customTypeSerializer: typeof(PrototypeIdSerializer<ConstructionPrototype>))]
    public string? Mirror;

    public IReadOnlyList<IConstructionCondition> Conditions => _conditions;
    public IReadOnlyList<SpriteSpecifier> Layers => _layers ?? new List<SpriteSpecifier> { Icon };
}

public enum ConstructionType
{
    Structure,
    Item,
}