// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿namespace Content.Shared.Movement.Pulling.Events;

public abstract class PullMessage : EntityEventArgs
{
    public readonly EntityUid PullerUid;
    public readonly EntityUid PulledUid;

    protected PullMessage(EntityUid pullerUid, EntityUid pulledUid)
    {
        PullerUid = pullerUid;
        PulledUid = pulledUid;
    }
}
