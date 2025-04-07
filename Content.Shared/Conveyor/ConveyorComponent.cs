// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Conveyor;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ConveyorComponent : Component
{
    /// <summary>
    ///     The angle to move entities by in relation to the owner's rotation.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public Angle Angle = Angle.Zero;

    /// <summary>
    ///     The amount of units to move the entity by per second.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public float Speed = 2f;

    /// <summary>
    ///     The current state of this conveyor
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public ConveyorState State;

    [ViewVariables, AutoNetworkedField]
    public bool Powered;

    [DataField]
    public ProtoId<SinkPortPrototype> ForwardPort = "Forward";

    [DataField]
    public ProtoId<SinkPortPrototype> ReversePort = "Reverse";

    [DataField]
    public ProtoId<SinkPortPrototype> OffPort = "Off";
}

[Serializable, NetSerializable]
public enum ConveyorVisuals : byte
{
    State
}

[Serializable, NetSerializable]
public enum ConveyorState : byte
{
    Off,
    Forward,
    Reverse
}
