// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 OnsenCapy <101037138+OnsenCapy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

namespace Content.Shared.Silicons.StationAi;

/// <summary>
/// Holds data for customizing the appearance of station AIs.
/// </summary>
[Prototype]
public sealed partial class StationAiCustomizationPrototype : IPrototype, IInheritingPrototype
{
    [IdDataField]
    public string ID { get; } = string.Empty;

    /// <summary>
    /// The (unlocalized) name of the customization.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    /// Stores the data which is used to modify the appearance of the station AI.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, PrototypeLayerData> LayerData = new();

    /// <summary>
    /// Key used to index the prototype layer data and extract a preview of the customization (for menus, etc)
    /// </summary>
    [DataField]
    public string PreviewKey = string.Empty;

    /// <summary>
    /// Specifies a background to use for previewing the customization (for menus, etc)
    /// </summary>
    [DataField]
    public SpriteSpecifier? PreviewBackground;

    /// <summary>
    /// The prototype we inherit from.
    /// </summary>
    [ViewVariables]
    [ParentDataFieldAttribute(typeof(AbstractPrototypeIdArraySerializer<StationAiCustomizationPrototype>))]
    public string[]? Parents { get; }

    /// <summary>
    /// Specifies whether the prototype is abstract.
    /// </summary>
    [ViewVariables]
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; }
}
