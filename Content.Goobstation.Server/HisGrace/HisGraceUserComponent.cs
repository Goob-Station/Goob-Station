// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.HisGrace;

[RegisterComponent]
public sealed partial class HisGraceUserComponent : Component
{
    /// <summary>
    ///  The speed multiplier of His Grace.
    /// </summary>
    [DataField]
    public float SpeedMultiplier = 1.2f;

}
