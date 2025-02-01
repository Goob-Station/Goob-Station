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
/// This event is raised on entities that got a new disease.
/// </summary>
public sealed class DiseaseGainedEvent : EntityEventArgs
{
    public Entity<DiseaseComponent> DiseaseGained;

    public DiseaseGainedEvent(Entity<DiseaseComponent> ent)
    {
        DiseaseGained = ent;
    }
}

/// <summary>
/// This event is raised on entities which just lost a disease.
/// </summary>
public sealed class DiseaseCuredEvent : EntityEventArgs
{
    public Entity<DiseaseComponent> DiseaseCured;

    public DiseaseCuredEvent(Entity<DiseaseComponent> ent)
    {
        DiseaseCured = ent;
    }
}

/// <summary>
/// This event is raised on an entity just before it's infected. Set <see cref="CanInfect"/> to false to prevent the infection.
/// </summary>
[ByRefEvent]
public sealed class DiseaseInfectAttemptEvent : EntityEventArgs
{
    public Entity<DiseaseComponent> Disease;
    public bool CanInfect = true;

    public DiseaseInfectAttemptEvent(Entity<DiseaseComponent> ent)
    {
        Disease = ent;
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
