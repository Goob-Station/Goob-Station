// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 solsticeofthywinter <wrendelphinelowing@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;

namespace Content.Goobstation.Shared.Traits.Components;

[RegisterComponent]
public sealed partial class MovementImpairedCorrectionComponent : Component
{
    /// <summary>
    /// How much should the impaired speed be fixed by this component?
    /// </summary>
    /// <remarks>
    /// Values between 0 and 1 determine how much of the impairment is corrected.
    /// If set to zero, removes the impaired speed entirely.
    /// </remarks>
    [DataField]
    public FixedPoint2 SpeedCorrection;
}
