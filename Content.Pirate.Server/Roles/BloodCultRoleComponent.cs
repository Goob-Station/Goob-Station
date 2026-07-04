// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Roles;

namespace Content.Server.Roles;

/// <summary>
/// A Blood Cultist.
/// </summary>
[RegisterComponent]
public sealed partial class BloodCultRoleComponent : BaseMindRoleComponent
{
	/// <summary>
    ///     Stores captured blood.
    /// </summary>
    [DataField] public int Blood = 0;
}