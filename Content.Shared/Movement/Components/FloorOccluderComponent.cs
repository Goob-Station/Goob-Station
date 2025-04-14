// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Applies floor occlusion to any <see cref="FloorOcclusionComponent"/> that intersect us.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FloorOccluderComponent : Component
{

}