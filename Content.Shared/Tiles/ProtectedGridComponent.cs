// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Tiles;

/// <summary>
/// Prevents floor tile updates when attached to a grid.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(ProtectedGridSystem))]
public sealed partial class ProtectedGridComponent : Component
{
    /// <summary>
    /// A bitmask of all the initial tiles on this grid.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<Vector2i, ulong> BaseIndices = new();
}
