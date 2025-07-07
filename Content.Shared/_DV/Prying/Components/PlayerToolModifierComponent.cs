// SPDX-FileCopyrightText: 2025 Avalon <jfbentley1@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._DV.Prying.Components;

/// <summary>
/// Alters the interaction speed of attached entity's tools.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PlayerToolModifierComponent : Component
{
    /// <summary>
    /// Multiplies the time taken to perform a pry interaction on entities like
    /// airlocks and doors.
    /// <see cref="Shared.Prying.Components.GetPryTimeModifierEvent"/>
    /// </summary>
    [DataField]
    public float PryTimeMultiplier = 1.0f;
}
