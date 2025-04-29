// SPDX-FileCopyrightText: 2024 ScarKy0 <106310278+ScarKy0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Chemistry.Events;

/// <summary>
/// Raised directed on an entity when it embeds in another entity.
/// </summary>
[ByRefEvent]
public readonly record struct InjectOverTimeEvent(EntityUid embeddedIntoUid)
{
    /// <summary>
    /// Entity that is embedded in.
    /// </summary>
    public readonly EntityUid EmbeddedIntoUid = embeddedIntoUid;
}