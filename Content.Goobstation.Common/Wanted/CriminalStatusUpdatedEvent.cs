using Content.Shared.Security;

namespace Content.Goobstation.Common.Wanted;

/// <summary>
/// Broadcast event raised whenever a crew member's criminal status is applied to their in-world entity.
/// Fired after the HUD icon has already been updated.
/// </summary>
public sealed class CriminalStatusUpdatedEvent : EntityEventArgs
{
    public readonly EntityUid Entity;
    public readonly SecurityStatus Status;

    public CriminalStatusUpdatedEvent(EntityUid entity, SecurityStatus status)
    {
        Entity = entity;
        Status = status;
    }
}