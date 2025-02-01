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

/// <summary>
/// This event is raised on each disease effect entity on disease update.
/// </summary>
public sealed class DiseaseEffectEvent : EntityEventArgs
{
    /// <summary>
    /// The entity this effect should affect.
    /// </summary>
    public EntityUid Ent;
    public Entity<DiseaseComponent> Disease;
    public float EffectScale;

    public DiseaseEffectEvent(EntityUid ent, Entity<DiseaseComponent> disease, float scale)
    {
        Ent = ent;
        Disease = disease;
        EffectScale = scale;
    }
}

/// <summary>
/// This event is raised on a diseased entity to get its immune resistance.
/// </summary>
public sealed class DiseaseCuredEvent : EntityEventArgs
{
    public EntityUid DiseaseCured;

    public DiseaseCuredEvent (EntityUid ent)
    {
        DiseaseCured = ent;
    }
}

/// <summary>
/// This event is raised on a diseased entity to get its immune resistance.
/// </summary>
[ByRefEvent]
public sealed class GetImmunityEvent : EntityEventArgs
{
    public float ImmunityGainRate = 0f;
    public float ImmunityStrength = 0f;
}
