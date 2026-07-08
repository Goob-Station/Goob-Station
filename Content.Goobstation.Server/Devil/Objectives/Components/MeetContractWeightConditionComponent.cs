// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Devil.Contract;
using Content.Goobstation.Server.Devil.Objectives.Systems;

namespace Content.Goobstation.Server.Devil.Objectives.Components;

[RegisterComponent, Access(typeof(DevilContractSystem), typeof(DevilObjectiveSystem))]

public sealed partial class MeetContractWeightConditionComponent : Component
{
    [DataField]
    public int ContractWeight;
}
