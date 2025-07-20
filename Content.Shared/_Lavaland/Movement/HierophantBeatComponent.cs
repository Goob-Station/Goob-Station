// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
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
