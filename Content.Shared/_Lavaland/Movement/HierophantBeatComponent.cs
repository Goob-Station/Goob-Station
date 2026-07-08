// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Movement;

[RegisterComponent, NetworkedComponent]
public sealed partial class HierophantBeatComponent : Component
{
    [DataField]
    public float MovementSpeedBuff = 1.25f;

    [DataField]
    public ProtoId<AlertPrototype> HierophantBeatAlertId = "HierophantBeat";
}
