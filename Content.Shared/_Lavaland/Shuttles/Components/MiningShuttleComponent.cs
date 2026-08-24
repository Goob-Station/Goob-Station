// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Shuttles.Components;

/// <summary>
/// Marker component for the mining shuttle grid.
/// Used for lavaland's FTL whitelist.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MiningShuttleComponent : Component;