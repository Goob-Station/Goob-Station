// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
namespace Content.Goobstation.Shared._Trauma.AnimalAgeing.Components;

/// <summary>
/// When this entity ages up to the <see cref="AgeToChangeAt"/> the entity will be poly morphed into the <see cref="EntToSpawn"/>
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpawnEntityOnAgeUpComponent : Component
{
    [DataField]
    public List<EntProtoId> EntToSpawn;

    [DataField]
    public AnimalAgeState AgeToChangeAt = AnimalAgeState.Adult;
}
