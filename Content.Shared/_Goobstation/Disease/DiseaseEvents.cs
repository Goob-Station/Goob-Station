using System;
using System.Runtime.CompilerServices;

namespace Content.Shared.Disease;

/// <summary>
/// This event is raised on diseases on update.
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
/// This event is raised on disease effects on update.
/// </summary>
public sealed class DiseaseEffectEvent : EntityEventArgs
{
    /// <summary>
    /// The severity of the effect.
    /// Use for effects that set state.
    /// </summary>
    public float Severity;
    /// <summary>
    /// The severity of the effect adjusted for update interval. Is effectively seconds.
    /// Use for effects that adjust (over time) state.
    /// </summary>
    public TimeSpan TimeDelta;
    /// <summary>
    /// The entity this effect should affect.
    /// </summary>
    public EntityUid Ent;
    public Entity<DiseaseComponent> Disease;

    public DiseaseEffectEvent(EntityUid ent, Entity<DiseaseComponent> disease, float severity, TimeSpan delta)
    {
        Ent = ent;
        Disease = disease;
        Severity = severity;
        TimeDelta = delta;
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
    public readonly Entity<DiseaseComponent> Disease;
    public bool CanInfect = true;

    public DiseaseInfectAttemptEvent(Entity<DiseaseComponent> ent)
    {
        Disease = ent;
    }
}

/// <summary>
/// This event is raised on a disease effect just before it's triggered to check whether the effect should be triggered.
/// </summary>
[ByRefEvent]
public sealed class DiseaseCheckConditionsEvent : EntityEventArgs
{
    /// <summary>
    /// The severity of the effect.
    /// </summary>
    public readonly float Severity;
    /// <summary>
    /// The update interval of the effect.
    /// </summary>
    public readonly TimeSpan TimeDelta;
    public bool DoEffect = true;

    public DiseaseCheckConditionsEvent(float severity, TimeSpan delta)
    {
        Severity = severity;
        TimeDelta = delta;
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
