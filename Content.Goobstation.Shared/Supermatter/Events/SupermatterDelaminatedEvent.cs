namespace Content.Goobstation.Shared.Supermatter.Events;

public sealed class SupermatterDelaminatedEvent(EntityUid supermatter) : EntityEventArgs
{
    public readonly EntityUid Supermatter = supermatter;
}
