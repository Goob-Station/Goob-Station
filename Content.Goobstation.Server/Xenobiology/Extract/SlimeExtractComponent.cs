// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Xenobiology;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Xenobiology.Extract;

/// <summary>
/// Ideally, the extract component will hold an event that is fired on use.
/// </summary>
[RegisterComponent]
public sealed partial class SlimeExtractComponent : Component
{
    /// <summary>
    /// Has this extract been used already?
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool IsUsed;
}
