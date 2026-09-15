// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Server._Oskarrr.WildLands;

/// <summary>
/// Marks a Wild Lands / Lavaland map that already received fixed jockey + tribe placement.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WildLandsOpposingCampsPlacedComponent : Component;
