// SPDX-FileCopyrightText: 2025 Avalon <jfbentley1@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._DV.Damage.Components;

/// <summary>
/// Allows entities to have additional stamina damage for their melee
/// and weapon attacks.
/// <see cref="Shared.Damage.Events.StaminaMeleeHitEvent"/>
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BonusStaminaDamageComponent : Component
{
    /// <summary>
    /// Multiplies the stamina damage by this much during a stamina hit event
    /// </summary>
    [DataField]
    public float Multiplier = 1.25f;
}
