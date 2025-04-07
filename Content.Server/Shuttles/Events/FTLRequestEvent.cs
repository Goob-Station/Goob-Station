// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Shuttles.Events;

/// <summary>
/// Raised by a shuttle when it has requested an FTL.
/// </summary>
[ByRefEvent]
public record struct FTLRequestEvent(EntityUid MapUid);