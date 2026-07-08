// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;

namespace Content.Goobstation.Server.SlaughterDemon.Roles;

/// <summary>
///  Added to mind role entities to tag that they are a slaughter demon.
/// </summary>
[RegisterComponent, Access(typeof(SlaughterDemonSystem))]
public sealed partial class SlaughterDemonRoleComponent : BaseMindRoleComponent;
