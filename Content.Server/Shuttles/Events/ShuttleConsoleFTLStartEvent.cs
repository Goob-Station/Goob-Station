// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿namespace Content.Server.Shuttles.Events;

/// <summary>
/// Raised when shuttle console approved FTL
/// </summary>
[ByRefEvent]
public record struct ShuttleConsoleFTLTravelStartEvent(EntityUid Uid)
{
    public EntityUid Uid = Uid;
}
