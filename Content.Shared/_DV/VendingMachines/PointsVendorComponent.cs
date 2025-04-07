// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.GameStates;

namespace Content.Shared._DV.VendingMachines;

/// <summary>
/// Makes a <see cref="ShopVendorComponent"/> use mining points to buy items.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PointsVendorComponent : Component;