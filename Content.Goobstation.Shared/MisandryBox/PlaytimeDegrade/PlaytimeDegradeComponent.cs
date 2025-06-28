// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: MIT

using Content.Goobstation.Maths.FixedPoint;

namespace Content.Goobstation.Shared.MisandryBox.PlaytimeDegrade;

[RegisterComponent]
public sealed partial class PlaytimeDegradeComponent : Component
{
    [DataField]
    public int Since;

    [DataField]
    public int Until;

    [DataField]
    public float Floor = 0.5f;

    [DataField]
    public FixedPoint2? By;

    [DataField]
    public bool DisarmMalus = true;

    [ViewVariables]
    public float? DecayRatio;
}
