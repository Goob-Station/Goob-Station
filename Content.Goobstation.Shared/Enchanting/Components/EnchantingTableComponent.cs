// SPDX-FileCopyrightText: 2025 GoidaBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goidastation.Shared.Enchanting.Components;

/// <summary>
/// Marker component added to altars to let items be enchanted on them and allow mob sacrificing to upgrade tiers.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EnchantingTableComponent : Component;
