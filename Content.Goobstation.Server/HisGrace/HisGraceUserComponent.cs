// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.HisGrace;

[RegisterComponent]
public sealed partial class HisGraceUserComponent : Component
{
    /// <summary>
    ///  The current speed modifier of His Grace.
    /// </summary>
    [DataField]
    public float SpeedMultiplier = 1.2f;

    /// <summary>
    /// The initial speed modifier of His Grace.
    /// </summary>
    [DataField]
    public float BaseSpeedMultiplier = 1.2f;

    /// <summary>
    /// The UID of the His Grace this entity is linked to.
    /// </summary>
    [DataField]
    public EntityUid? HisGrace;
}
