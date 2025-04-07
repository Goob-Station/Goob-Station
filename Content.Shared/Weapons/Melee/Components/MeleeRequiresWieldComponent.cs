// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Wieldable;
using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Melee.Components;

/// <summary>
/// Indicates that this meleeweapon requires wielding to be useable.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedWieldableSystem))]
public sealed partial class MeleeRequiresWieldComponent : Component
{
    // Lavaland Change: The player will slip if they try to use the weapon without wielding it.
    [DataField]
    public bool FumbleOnAttempt = false;
}