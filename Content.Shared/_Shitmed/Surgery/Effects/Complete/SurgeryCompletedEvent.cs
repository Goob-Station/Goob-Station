// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Shitmed.Medical.Surgery.Effects.Complete;

/// <summary>
///     Raised on the entity that received the surgery.
/// </summary>
[ByRefEvent]
public record struct SurgeryCompletedEvent;