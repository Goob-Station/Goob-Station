// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿namespace Content.Shared.Interaction.Events
{
    public sealed class UseAttemptEvent(EntityUid uid, EntityUid used) : CancellableEntityEventArgs
    {
        public EntityUid Uid { get; } = uid;

        public EntityUid Used = used;
    }
}
