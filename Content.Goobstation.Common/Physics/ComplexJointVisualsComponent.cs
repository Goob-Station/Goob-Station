// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Common.Physics;

/// <summary>
/// Works like JointVisualsComponent, but supports multiple targets and more customization.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ComplexJointVisualsComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public Dictionary<NetEntity, ComplexJointVisualsData> Data = new(); // Target -> Data (no more than 1 beam per target)
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class ComplexJointVisualsData(
    string id,
    SpriteSpecifier sprite,
    Color color,
    SpriteSpecifier? startSprite = null,
    SpriteSpecifier? endSprite = null,
    TimeSpan? creationTime = null)
{
    public ComplexJointVisualsData() : this(string.Empty, SpriteSpecifier.Invalid, Color.White) { }

    public ComplexJointVisualsData(string id,
        SpriteSpecifier sprite,
        SpriteSpecifier? startSprite = null,
        SpriteSpecifier? endSprite = null,
        TimeSpan? creationTime = null) : this(id, sprite, Color.White, startSprite, endSprite, creationTime)
    {
    }

    [DataField]
    public SpriteSpecifier? StartSprite = startSprite;

    [DataField]
    public SpriteSpecifier? EndSprite = endSprite;

    [DataField]
    public SpriteSpecifier Sprite = sprite;

    [DataField]
    public Color Color = color;

    [DataField]
    public string Id = id;

    [DataField]
    public TimeSpan? CreationTime = creationTime;

    [DataField]
    public Vector2 Scale = Vector2.One;

    // TODO: add support for joint offsets
}
