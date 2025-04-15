// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DoutorWhite <thedoctorwhite@gmail.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Light.Components;

/// <summary>
/// Counts the tile this entity on as being rooved.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class IsRoofComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    /// Color for this roof. If null then falls back to the grid's color.
    /// </summary>
    /// <remarks>
    /// If a tile is marked as rooved then the tile color will be used over any entity's colors on the tile.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public Color? Color;
}
