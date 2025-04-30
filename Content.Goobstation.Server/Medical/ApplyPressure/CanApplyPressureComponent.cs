// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Goobstation.Server.Medical.ApplyPressure;

[RegisterComponent]
public sealed partial class CanApplyPressureComponent : Component
{
    /// <summary>
    /// How long each pressure application takes to finish.
    /// </summary>
    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The amount bleed is modified by per pressure application.
    /// </summary>
    [DataField]
    public float BleedModifier = -1.6f;
}
