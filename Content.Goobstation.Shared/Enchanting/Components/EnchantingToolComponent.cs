// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Component added to bibles that lets them interact with <see cref="EnchanterComponent"/>
/// on an altar to enchant an item.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EnchantingToolComponent : Component;
