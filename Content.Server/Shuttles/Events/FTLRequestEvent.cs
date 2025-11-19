// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.Shuttles.Events;

/// <summary>
/// Raised by a shuttle when it has requested an FTL.
/// </summary>
[ByRefEvent]
public record struct FTLRequestEvent(EntityUid MapUid);
