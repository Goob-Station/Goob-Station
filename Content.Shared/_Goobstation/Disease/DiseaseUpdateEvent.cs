namespace Content.Shared.Disease;

/// <summary>
/// This event is raised when a DiseaseComponent is updated.
/// </summary>
public sealed class DiseaseUpdateEvent : EntityEventArgs
{
    public EntityUid Ent;

    public DiseaseUpdateEvent(EntityUid ent)
    {
        Ent = ent;
    }
}
