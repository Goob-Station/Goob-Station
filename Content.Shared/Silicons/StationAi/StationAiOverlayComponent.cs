// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Silicons.StationAi;

/// <summary>
/// Handles the static overlay for station AI.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState] // Shitmed Change - Starlight Abductors
public sealed partial class StationAiOverlayComponent : Component
{
    /// <summary>
    ///     Shitmed Change - Starlight Abductors: Whether the station AI overlay should be allowed to cross grids.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AllowCrossGrid;
}