// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.SlaughterDemon.Items;

/// <summary>
/// This is used for tracking objectives
/// </summary>
[RegisterComponent]
public sealed partial class VialSummonComponent : Component
{
    /// <summary>
    ///  The entity who summoned an entity from the vial
    /// </summary>
    [DataField]
    public EntityUid? Summoner;

    /// <summary>
    ///  Ensures we get the objective only for that wizard.
    /// </summary>
    [DataField]
    public bool Used;
}
