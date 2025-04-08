// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.DeviceLinking.Components;

/// <summary>
/// Simple ternary state for device linking.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TwoWayLeverComponent : Component
{
    [DataField, AutoNetworkedField]
    public TwoWayLeverState State;

    [DataField, AutoNetworkedField]
    public bool NextSignalLeft;

    [DataField]
    public ProtoId<SourcePortPrototype> LeftPort = "Left";

    [DataField]
    public ProtoId<SourcePortPrototype> RightPort = "Right";

    [DataField]
    public ProtoId<SourcePortPrototype> MiddlePort = "Middle";
}