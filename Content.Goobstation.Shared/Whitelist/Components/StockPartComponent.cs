// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Goobstation.Shared.Whitelist.Components;

/// <summary>
/// Whitelist component for stock parts to avoid tag redefinition and collisions
/// </summary>
[RegisterComponent]
public sealed partial class StockPartComponent : Component;