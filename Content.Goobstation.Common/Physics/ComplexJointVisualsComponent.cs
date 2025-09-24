// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Common.Physics;

/// <summary>
/// Works like JointVisualsComponent, but supports multiple targets and more customization.
/// Sometimes it works not good enough though, so use this only when necessary.
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
    Color color)
{
    public ComplexJointVisualsData() : this(string.Empty, SpriteSpecifier.Invalid, Color.White) { }

    public ComplexJointVisualsData(string id, SpriteSpecifier sprite) : this(id, sprite, Color.White) { }

    [DataField]
    public SpriteSpecifier Sprite = sprite;

    [DataField]
    public Color Color = color;

    [DataField]
    public string Id = id;

    // TODO: add support for joint offsets
}
