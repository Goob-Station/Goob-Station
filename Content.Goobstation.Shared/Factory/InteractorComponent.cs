// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Containers.ItemSlots;
using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Factory;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedInteractorSystem))]
[AutoGenerateComponentState]
public sealed partial class InteractorComponent : Component
{
    [DataField]
    public string ToolContainerId = "interactor_tool";

    /// <summary>
    /// Signal port to toggle or enable/disable <see cref="AltInteract"/>.
    /// </summary>
    [DataField]
    public ProtoId<SinkPortPrototype> AltInteractPort = "AltInteract";

    /// <summary>
    /// Whether to use alt interaction, i.e. use the highest priority verb on the target entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AltInteract;
}

[Serializable, NetSerializable]
public enum InteractorVisuals : byte
{
    State
}

[Serializable, NetSerializable]
public enum InteractorLayers : byte
{
    Hand,
    Powered
}

[Serializable, NetSerializable]
public enum InteractorState : byte
{
    // Inactive with no tool
    Empty,
    // Inactive with a tool
    Inactive,
    // Active, with or without a tool
    Active
}
