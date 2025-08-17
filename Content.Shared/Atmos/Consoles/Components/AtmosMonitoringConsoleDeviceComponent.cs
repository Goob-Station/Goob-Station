// SPDX-FileCopyrightText: 2024 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Atmos.Components;

/// <summary>
/// Entities with this component appear on the
/// nav maps of atmos monitoring consoles
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AtmosMonitoringConsoleDeviceComponent : Component
{
    /// <summary>
    /// Prototype ID for the blip used to represent this
    /// entity on the atmos monitoring console nav map.
    /// If null, no blip is drawn (i.e., null for pipes)
    /// </summary>
    [DataField]
    public ProtoId<NavMapBlipPrototype>? NavMapBlip = null;

    /// <summary>
    /// Sets whether attached atmos pipes will always be rendered
    /// on the atmos monitoring console nav map, even if these
    /// pipes are not connected to any pipes in a neighboring tile.
    /// </summary>
    [DataField]
    public bool ShowAbsentConnections = true;
}
