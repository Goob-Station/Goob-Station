// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;

namespace Content.Shared._Shitmed.Body.Events;

/// <summary>
/// Raised on an entity when attempting to remove a body part.
/// </summary>
[ByRefEvent]
public readonly record struct AmputateAttemptEvent(EntityUid Part);

// Kind of a clone of BodyPartAddedEvent for surgical reattachment specifically.
[ByRefEvent]
public readonly record struct BodyPartAttachedEvent(Entity<BodyPartComponent> Part);

// Kind of a clone of BodyPartRemovedEvent for any instances where we call DropPart(), reasoning being that RemovedEvent fires off
// a lot more often than what I'd like due to PVS.
[ByRefEvent]
public readonly record struct BodyPartDroppedEvent(Entity<BodyPartComponent> Part);

[ByRefEvent]
public readonly record struct BodyPartEnableChangedEvent(bool Enabled);

[ByRefEvent]
public readonly record struct BodyPartEnabledEvent(Entity<BodyPartComponent> Part);

[ByRefEvent]
public readonly record struct BodyPartDisabledEvent(Entity<BodyPartComponent> Part);

public readonly record struct BodyPartComponentsModifyEvent(EntityUid Body, bool Add);