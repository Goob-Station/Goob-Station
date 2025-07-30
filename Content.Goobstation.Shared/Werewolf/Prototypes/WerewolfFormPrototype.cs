// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Werewolf.Prototypes;

[Prototype]
[DataDefinition]
public sealed partial class WerewolfFormPrototype : IPrototype, IInheritingPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<WerewolfFormPrototype>))]
    public string[]? Parents { get; private set; }

    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }

    [DataField(required: true, serverOnly: true)]
    public WerewolfConfiguration Configuration = new();
}

/// <summary>
/// Defines information about the werewolf
/// </summary>
[DataDefinition]
public sealed partial record WerewolfConfiguration
{
    /// <summary>
    /// The color of the werewolf's fur
    /// </summary>
    [DataField(required: true)]
    public Color FurColor;

    /// <summary>
    /// The entity to transform into
    /// </summary>
    [DataField(required: true)]
    public EntProtoId? Entity;

    /// <summary>
    /// The name of the form
    /// </summary>
    [DataField]
    public string? Name = string.Empty;

    /// <summary>
    /// The sprite of the werewolf's form. Used in ui
    /// </summary>
    [DataField]
    public SpriteSpecifier? Sprite;

    /// <summary>
    /// The components to add to the werewolf
    /// </summary>
    [DataField(serverOnly: true)]
    public ComponentRegistry Components = new();

    /// <summary>
    /// How much the form costs in fury
    /// </summary>
    [DataField]
    public int FuryCost = 15;
}
