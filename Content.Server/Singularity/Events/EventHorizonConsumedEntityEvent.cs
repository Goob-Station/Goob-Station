// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 keronshb <keronshb@live.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Singularity.Components;
using Robust.Shared.Containers;

namespace Content.Server.Singularity.Events;

/// <summary>
/// Event raised on the entity being consumed whenever an event horizon consumes an entity.
/// </summary>
[ByRefEvent]
public readonly record struct EventHorizonConsumedEntityEvent
(EntityUid entity, EntityUid eventHorizonUid, EventHorizonComponent eventHorizon, BaseContainer? container)
{
    /// <summary>
    /// The entity being consumed by the event horizon.
    /// </summary>
    public readonly EntityUid Entity = entity;

    /// <summary>
    /// The uid of the event horizon consuming the entity.
    /// </summary>
    public readonly EntityUid EventHorizonUid = eventHorizonUid;

    /// <summary>
    /// The event horizon consuming the target entity.
    /// </summary>
    public readonly EventHorizonComponent EventHorizon = eventHorizon;

    /// <summary>
    /// The innermost container of the entity being consumed by the event horizon that is not also in the process of being consumed by the event horizon.
    /// Used to correctly dump out the contents containers that are consumed by the event horizon.
    /// </summary>
    public readonly BaseContainer? Container = container;
}