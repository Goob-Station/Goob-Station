// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.Physics;

/// <summary>
/// Just draws a generic line between this entity and the target.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class JointVisualsComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("sprite", required: true), AutoNetworkedField]
    public SpriteSpecifier Sprite = default!;

    [ViewVariables(VVAccess.ReadWrite), DataField("target"), AutoNetworkedField]
    public NetEntity? Target;

    /// <summary>
    /// Offset from Body A.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("offsetA"), AutoNetworkedField]
    public Vector2 OffsetA;

    /// <summary>
    /// Offset from Body B.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("offsetB"), AutoNetworkedField]
    public Vector2 OffsetB;
}