// SPDX-FileCopyrightText: 2025 Avalon <jfbentley1@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Weapons.Ranged.Components;

/// <summary>
/// Alters the accuracy of attached entity's held or wielded guns via
/// <see cref="Shared.Weapons.Ranged.Events.GunRefreshModifiersEvent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PlayerAccuracyModifierComponent : Component
{
    /// <summary>
    /// Multiplies the Min/Max angles of a gun by this amount.
    /// </summary>
    [DataField]
    public float SpreadMultiplier = 15f;

    /// <summary>
    /// Modifies the Min angle of a gun by this amount.
    /// </summary>
    [DataField]
    public float MinSpreadModifier = 10f;

    /// <summary>
    /// Modifies the Max angle of a gun by this amount.
    /// </summary>
    [DataField]
    public float MaxSpreadModifier = 20f;

    /// <summary>
    /// Maximum angle, in degrees, an entity can shoot between.
    /// After the SpreadMultiplier is applied, this clamp can stop the entity
    /// from shooting behind themselves.
    /// </summary>
    [DataField]
    public float MaxSpreadAngle = 180f;

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;
}
