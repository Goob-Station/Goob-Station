// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Component checked for by EnchanterSystem before enchanting.
/// Place this on any entity you want to allow to enchant... e.g. Chaplain, Heretic or Wizard.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CanEnchantComponent : Component;
