// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Chemistry.GunApplySolution;

[RegisterComponent]
public sealed partial class GunApplySolutionComponent : Component
{
    [DataField]
    public string SourceSolution = "solution";

    [DataField]
    public string TargetSolution = "ammo";

    [DataField]
    public float Amount = 5f;
}
