// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Mobs;

/// <summary>
///     Marker for whether a mob is fauna.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FaunaComponent : Component;
