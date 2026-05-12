// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.AnimalAgeing.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpawnEntityOnOldAgeDeathComponent : Component
{
    [DataField]
    public EntProtoId HappyDeathEnt;

    [DataField]
    public EntProtoId SadDeathEnt;

    [DataField]
    public float HappinessRequired = 30f;

    [DataField]
    public float UnHappinessRequired;
}
