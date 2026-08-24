// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons.Ranged.Components;

/// <summary>
///     Component to indicate a valid flashlight for weapon attachment
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GunUpgradeFlashlightComponent : Component;
