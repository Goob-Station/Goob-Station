using Content.Shared.Inventory;
using Robust.Shared.Prototypes;
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
/// This event is raised on disease effects when triggered.
/// </summary>
public sealed class DiseaseEffectEvent : EntityEventArgs
{
    /// <summary>
    /// The DiseaseEffectComponent of the effect entity this is fired on.
    /// </summary>
    public readonly DiseaseEffectComponent Comp;

    /// <summary>
    /// The entity this effect should affect.
    /// </summary>
    public readonly EntityUid Ent;

    /// <summary>
    /// The host disease of this effect.
    /// </summary>
    public readonly Entity<DiseaseComponent> Disease;

    public DiseaseEffectEvent(EntityUid ent, Entity<DiseaseComponent> disease, DiseaseEffectComponent comp)
    {
        Ent = ent;
        Disease = disease;
        Comp = comp;
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
/// Raised on a newly created base disease entity to clone the provided entity onto it.
/// </summary>
public sealed class DiseaseCloneEvent : EntityEventArgs
{
    public Entity<DiseaseComponent> Source;

    public DiseaseCloneEvent(Entity<DiseaseComponent> ent)
    {
        Source = ent;
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
    /// The DiseaseEffectComponent of the effect entity this is fired on.
    /// </summary>
    public readonly DiseaseEffectComponent Comp;

    /// <summary>
    /// The entity this effect should affect.
    /// </summary>
    public readonly EntityUid Ent;

    /// <summary>
    /// The host disease of this effect.
    /// </summary>
    public readonly Entity<DiseaseComponent> Disease;

    /// <summary>
    /// Whether this effect should fire.
    /// </summary>
    public bool DoEffect = true;

    public DiseaseCheckConditionsEvent(EntityUid ent, Entity<DiseaseComponent> disease, DiseaseEffectComponent comp)
    {
        Ent = ent;
        Disease = disease;
        Comp = comp;
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

/// <summary>
/// This event is raised on an entity from which a disease is trying to spread just before it attempts to do so.
/// </summary>
[ByRefEvent]
public record struct DiseaseOutgoingSpreadAttemptEvent(float Power, float Chance, ProtoId<DiseaseSpreadPrototype> Type) : IInventoryRelayEvent
{
    public float Power = Power;
    public float Chance = Chance;
    public readonly ProtoId<DiseaseSpreadPrototype> Type = Type;

    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}

/// <summary>
/// This event is raised on an entity to which a disease is trying to spread just before it attempts to do so.
/// </summary>
[ByRefEvent]
public record struct DiseaseIncomingSpreadAttemptEvent(float Power, float Chance, ProtoId<DiseaseSpreadPrototype> Type) : IInventoryRelayEvent
{
    public float Power = Power;
    public float Chance = Chance;
    public readonly ProtoId<DiseaseSpreadPrototype> Type = Type;

    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}
