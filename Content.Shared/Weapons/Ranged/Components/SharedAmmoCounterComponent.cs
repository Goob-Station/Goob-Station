// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Ranged.Components;

/// <summary>
/// Shows an ItemStatus with the ammo of the gun. Adjusts based on what the ammoprovider is.
/// </summary>
[NetworkedComponent]
public abstract partial class SharedAmmoCounterComponent : Component {}