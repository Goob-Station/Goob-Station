// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Electrocution;

/// <summary>
/// Updates every frame for short duration to check if electrifed entity is powered when activated, e.g to play animation
/// </summary>
[RegisterComponent]
public sealed partial class ActivatedElectrifiedComponent : Component
{
    /// <summary>
    /// How long electrified entity will remain active
    /// </summary>
    [ViewVariables]
    public float TimeLeft = 1f;
}