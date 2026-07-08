// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Interaction.Events
{
    public sealed class UseAttemptEvent(EntityUid uid, EntityUid used) : CancellableEntityEventArgs
    {
        public EntityUid Uid { get; } = uid;

        public EntityUid Used = used;
    }
}