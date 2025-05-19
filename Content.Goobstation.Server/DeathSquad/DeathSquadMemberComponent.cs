// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;

namespace Content.Goobstation.Server.DeathSquad;

[RegisterComponent]
public sealed partial class DeathSquadMemberComponent : Component
{
    /// <summary>
    /// The amount added to this entities critical and dead states.
    /// </summary>
    [DataField]
    public FixedPoint2 NewHealth = 250;
}
