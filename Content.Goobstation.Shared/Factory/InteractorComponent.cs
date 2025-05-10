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
[AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class InteractorComponent : Component
{
    [DataField]
    public string ToolContainerId = "interactor_tool";

    /// <summary>
    /// Fixture to look for target items with.
    /// </summary>
    [DataField]
    public string TargetFixtureId = "interactor_target";

    /// <summary>
    /// Entities currently colliding with <see cref="TargetFixtureId"/> and whether their CollisionWake was enabled.
    /// When entities start to collide they get pushed to the end.
    /// When picking up items the last value is taken.
    /// This is essentially a FILO queue.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<(NetEntity, bool)> TargetEntities = new();
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
