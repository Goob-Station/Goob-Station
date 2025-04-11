// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared.Mech;

[Serializable, NetSerializable]
public enum MechVisuals : byte
{
    Open, //whether or not it's open and has a rider
    Broken //if it broke and no longer works.
}

[Serializable, NetSerializable]
public enum MechAssemblyVisuals : byte
{
    State
}

[Serializable, NetSerializable]
public enum MechVisualLayers : byte
{
    Base
}

/// <summary>
/// Event raised on equipment when it is inserted into a mech
/// </summary>
[ByRefEvent]
public readonly record struct MechEquipmentInsertedEvent(EntityUid Mech)
{
    public readonly EntityUid Mech = Mech;
}

/// <summary>
/// Event raised on equipment when it is removed from a mech
/// </summary>
[ByRefEvent]
public readonly record struct MechEquipmentRemovedEvent(EntityUid Mech)
{
    public readonly EntityUid Mech = Mech;
}

/// <summary>
/// Raised on the mech equipment before it is going to be removed.
/// </summary>
[ByRefEvent]
public record struct AttemptRemoveMechEquipmentEvent()
{
    public bool Cancelled = false;
}

public sealed partial class MechToggleEquipmentEvent : InstantActionEvent
{
}

public sealed partial class MechOpenUiEvent : InstantActionEvent
{
}

public sealed partial class MechEjectPilotEvent : InstantActionEvent
{
}