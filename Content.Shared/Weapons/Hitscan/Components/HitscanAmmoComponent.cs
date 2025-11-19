// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Shared.Weapons.Ranged;
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Hitscan.Components;

/// <summary>
/// This component is used to indicate an entity is shootable from a hitscan weapon.
/// This is placed on the laser entity being shot, not the gun itself.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HitscanAmmoComponent : Component, IShootable;
