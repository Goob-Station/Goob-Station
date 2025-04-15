// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Body.Events;

// All of these events are raised on a mechanism entity when added/removed to a body in different
// ways.

/// <summary>
/// Raised on a mechanism when it is added to a body part.
/// </summary>
[ByRefEvent]
public readonly record struct OrganAddedEvent(EntityUid Part);

/// <summary>
/// Raised on a mechanism when it is added to a body part within a body.
/// </summary>
[ByRefEvent]
public readonly record struct OrganAddedToBodyEvent(EntityUid Body, EntityUid Part);

/// <summary>
/// Raised on a mechanism when it is removed from a body part.
/// </summary>
[ByRefEvent]
public readonly record struct OrganRemovedEvent(EntityUid OldPart);

/// <summary>
/// Raised on a mechanism when it is removed from a body part within a body.
/// </summary>
[ByRefEvent]
public readonly record struct OrganRemovedFromBodyEvent(EntityUid OldBody, EntityUid OldPart);