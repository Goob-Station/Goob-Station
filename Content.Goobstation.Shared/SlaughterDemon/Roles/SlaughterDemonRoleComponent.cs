// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;
using Content.Shared.Roles.Components;

namespace Content.Goobstation.Shared.SlaughterDemon.Roles;

/// <summary>
///  Added to mind role entities to tag that they are a slaughter demon.
/// </summary>
[RegisterComponent]
public sealed partial class SlaughterDemonRoleComponent : BaseMindRoleComponent;
