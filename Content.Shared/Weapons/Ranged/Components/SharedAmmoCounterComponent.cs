// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// Shows an ItemStatus with the ammo of the gun. Adjusts based on what the ammoprovider is.
/// </summary>
[NetworkedComponent]
public abstract partial class SharedAmmoCounterComponent : Component {}
