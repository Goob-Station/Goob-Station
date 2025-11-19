// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Interaction.Events;

public sealed class ChangeDirectionAttemptEvent : CancellableEntityEventArgs
{
    public ChangeDirectionAttemptEvent(EntityUid uid)
    {
        Uid = uid;
    }

    public EntityUid Uid { get; }
}
