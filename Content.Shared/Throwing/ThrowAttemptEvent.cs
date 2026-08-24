// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Throwing
{
    public sealed class ThrowAttemptEvent : CancellableEntityEventArgs
    {
        public ThrowAttemptEvent(EntityUid uid, EntityUid itemUid,
            EntityUid? targetUid // Goob pass along target
            )
        {
            Uid = uid;
            ItemUid = itemUid;
            TargetUid = targetUid;  // Goob pass along target
        }

        public EntityUid Uid { get; }

        public EntityUid ItemUid { get; }

        public EntityUid? TargetUid { get; } // Goob pass along target
    }

    /// <summary>
    ///     Raised on the item entity that is thrown.
    /// </summary>
    /// <param name="User">The user that threw this entity.</param>
    /// <param name="Cancelled">Whether or not the throw should be cancelled.</param>
    [ByRefEvent]
    public record struct ThrowItemAttemptEvent(EntityUid User, bool Cancelled = false);

    /// <summary>
    /// Raised when we try to pushback an entity from throwing
    /// </summary>
    public sealed class ThrowPushbackAttemptEvent : CancellableEntityEventArgs {}

    /// <summary>
    /// Raised on an entity that is being pushed from a thrown entity
    /// </summary>
    [ByRefEvent]
    public record struct ThrowerImpulseEvent()
    {
        public bool Push;
    };
}
