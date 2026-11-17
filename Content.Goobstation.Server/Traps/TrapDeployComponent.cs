// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Traps;

[RegisterComponent]
public sealed partial class TrapDeployComponent : Component
{
    [DataField(required: true)]
    public EntProtoId DeployedPrototype;
}

[RegisterComponent]
public sealed partial class TrapPickupComponent : Component
{
    [DataField(required: true)]
    public EntProtoId ItemPrototype;
}
