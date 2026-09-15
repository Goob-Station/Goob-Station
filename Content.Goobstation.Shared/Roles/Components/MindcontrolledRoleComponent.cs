// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles.Components;

namespace Content.Goobstation.Shared.Roles.Components;

[RegisterComponent]
public sealed partial class MindcontrolledRoleComponent : BaseMindRoleComponent
{
    [DataField] public EntityUid? MasterUid = null;
}
