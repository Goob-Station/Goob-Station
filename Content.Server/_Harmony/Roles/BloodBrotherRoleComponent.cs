// SPDX-FileCopyrightText: 2025 youtissoum <51883137+youtissoum@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;

namespace Content.Server._Harmony.Roles;

/// <summary>
/// Added to mind role entities to tag that they are a blood brother.
/// </summary>
[RegisterComponent]
public sealed partial class BloodBrotherRoleComponent : BaseMindRoleComponent
{
    [DataField]
    public EntityUid? Brother;
}
