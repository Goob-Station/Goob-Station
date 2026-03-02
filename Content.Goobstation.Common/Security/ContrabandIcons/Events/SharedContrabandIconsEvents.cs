namespace Content.Goobstation.Common.Security.ContrabandIcons.Events;



/// <param name="IdCardUid">The entity UID of the ID card that was inserted.</param>
/// <param name="TargetUid">The entity UID of the Pda.</param>
public record struct IdCardInsertedEvent(EntityUid IdCardUid, EntityUid TargetUid);

/// <param name="IdCardUid">The entity UID of the ID card that was removed.</param>
/// <param name="TargetUid">The entity UID of the Pda.</param>
public record struct IdCardRemovedEvent(EntityUid IdCardUid, EntityUid TargetUid);