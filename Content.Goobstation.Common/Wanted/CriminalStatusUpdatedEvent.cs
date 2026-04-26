namespace Content.Goobstation.Common.Wanted;

/// <summary>
/// Broadcast event raised whenever a crew member's criminal status is applied to their in-world entity.
/// Fired after the HUD icon has already been updated.
/// Status is stored as byte because Common cannot reference Content.Shared (where SecurityStatus lives).
/// Cast to SecurityStatus in consumers.
/// </summary>
public sealed class CriminalStatusUpdatedEvent : EntityEventArgs
{
    public readonly EntityUid Entity;

    /// <summary>
    /// The new security status as a byte. Cast to <c>SecurityStatus</c> in systems that need it.
    /// </summary>
    public readonly byte Status;

    public CriminalStatusUpdatedEvent(EntityUid entity, byte status)
    {
        Entity = entity;
        Status = status;
    }
}