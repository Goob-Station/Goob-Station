// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._DV.Whitelist;

/// <summary>
/// Marker component for holofans, used for reclaiming charges of the projector.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HolofanComponent : Component;
