// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Added to a mind or mob to prevent it upgrading enchanted items when killed.
/// Gets added to both after a successful sacrifice.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SoullessComponent : Component;
