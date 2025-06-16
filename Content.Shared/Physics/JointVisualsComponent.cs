// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Physics;

/// <summary>
/// Just draws a generic line between this entity and the target.
/// Goobstation rework: now supports multiple joints
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class JointVisualsComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public Dictionary<NetEntity, JointVisualsData> Data = new(); // Target -> Data (no more than 1 beam per target)
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class JointVisualsData(
    SpriteSpecifier sprite,
    Color color)
{
    public JointVisualsData() : this(SpriteSpecifier.Invalid, Color.White) { }

    public JointVisualsData(SpriteSpecifier sprite) : this(sprite, Color.White) { }

    [DataField]
    public SpriteSpecifier Sprite = sprite;

    [DataField]
    public Color Color = color;

    // TODO: add support for joint offsets
}
