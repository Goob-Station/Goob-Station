// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.ItemUpgrades.Components;

/// <summary>
/// Allows the entity to support item slot upgrades.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ItemUpgradesSystem))]
public sealed partial class ItemUpgradeableComponent : Component;
