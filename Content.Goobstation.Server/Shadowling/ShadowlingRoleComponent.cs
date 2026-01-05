// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;

namespace Content.Goobstation.Server.Shadowling;

/// <summary>
/// Added to mind role entities to tag that they are a shadowling.
/// </summary>
[RegisterComponent]
public sealed partial class ShadowlingRoleComponent : BaseMindRoleComponent
{
    /// <summary>
    /// Used for round-end text. Indicates how many thralls the Shadowling converted during the round.
    /// </summary>
    [DataField]
    public int ThrallsConverted;
}
