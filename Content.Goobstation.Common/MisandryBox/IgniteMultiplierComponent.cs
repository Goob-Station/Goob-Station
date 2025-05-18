// SPDX-FileCopyrightText: 2025 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Common.MisandryBox;

[RegisterComponent]
public sealed partial class IgniteMultiplierComponent : Component
{
    /// <summary>
    /// Multiply received burn stacks by this value
    /// </summary>
    [DataField]
    public float Factor { get; set; } = 2;
}
