using Content.Shared.Inventory;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease;

/// <summary>
/// This event is raised on diseases on update.
/// </summary>
public sealed class DiseaseUpdateEvent(Entity<Components.DiseaseCarrierComponent> ent) : EntityEventArgs
{
    public Entity<Components.DiseaseCarrierComponent> Ent = ent;
}

/// <summary>
/// This event is raised on disease effects when triggered.
/// </summary>
public sealed class DiseaseEffectEvent(EntityUid ent, Entity<Components.DiseaseComponent> disease, Components.DiseaseEffectComponent comp) : EntityEventArgs
{
    /// <summary>
    /// The DiseaseEffectComponent of the effect entity this is fired on.
    /// </summary>
    public readonly Components.DiseaseEffectComponent Comp = comp;

    /// <summary>
    /// The entity this effect should affect.
    /// </summary>
    public readonly EntityUid Ent = ent;

    /// <summary>
    /// The host disease of this effect.
    /// </summary>
    public readonly Entity<Components.DiseaseComponent> Disease = disease;
}

/// <summary>
/// This event is raised on entities that got a new disease.
/// </summary>
public sealed class DiseaseGainedEvent(Entity<Components.DiseaseComponent> ent) : EntityEventArgs
{
    public Entity<Components.DiseaseComponent> DiseaseGained = ent;
}

/// <summary>
/// This event is raised on entities which just lost a disease.
/// </summary>
public sealed class DiseaseCuredEvent(Entity<Components.DiseaseComponent> ent) : EntityEventArgs
{
    public Entity<Components.DiseaseComponent> DiseaseCured = ent;
}

/// <summary>
/// Raised on a newly created base disease entity to clone the provided entity onto it.
/// </summary>
public sealed class DiseaseCloneEvent(Entity<Components.DiseaseComponent> ent) : EntityEventArgs
{
    public Entity<Components.DiseaseComponent> Source = ent;
}

/// <summary>
/// This event is raised on an entity just before it's infected. Set <see cref="CanInfect"/> to false to prevent the infection.
/// </summary>
[ByRefEvent]
public sealed class DiseaseInfectAttemptEvent(Entity<Components.DiseaseComponent> ent) : EntityEventArgs
{
    public readonly Entity<Components.DiseaseComponent> Disease = ent;
    public bool CanInfect = true;
}

/// <summary>
/// This event is raised on a disease effect just before it's triggered to check whether the effect should be triggered.
/// </summary>
[ByRefEvent]
public sealed class DiseaseCheckConditionsEvent(
    EntityUid ent,
    Entity<Components.DiseaseComponent> disease,
    Components.DiseaseEffectComponent comp)
    : EntityEventArgs
{
    /// <summary>
    /// The DiseaseEffectComponent of the effect entity this is fired on.
    /// </summary>
    public readonly Components.DiseaseEffectComponent Comp = comp;

    /// <summary>
    /// The entity this effect should affect.
    /// </summary>
    public readonly EntityUid Ent = ent;

    /// <summary>
    /// The host disease of this effect.
    /// </summary>
    public readonly Entity<Components.DiseaseComponent> Disease = disease;

    /// <summary>
    /// Whether this effect should fire.
    /// </summary>
    public bool DoEffect = true;
}

/// <summary>
/// This event is raised on a diseased entity to get its immune resistance.
/// </summary>
[ByRefEvent]
public sealed class GetImmunityEvent(Entity<Components.DiseaseComponent> disease) : EntityEventArgs
{
    public readonly Entity<Components.DiseaseComponent> Disease = disease;

    public float ImmunityGainRate = 0f;
    public float ImmunityStrength = 0f;
}

/// <summary>
/// Base event for disease spread attempts.
/// </summary>
public abstract record DiseaseSpreadAttemptEvent(float Power, float Chance, ProtoId<DiseaseSpreadPrototype> Type) : IInventoryRelayEvent
{
    public float Power { get; set; } = Power;
    public float Chance { get; set; } = Chance;
    public ProtoId<DiseaseSpreadPrototype> Type { get; } = Type;

    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;

    public void ApplyModifier(DiseaseSpreadModifier mod)
    {
        Power += mod.PowerMod(Type);
        Chance *= mod.ChanceMult(Type);
    }
}

/// <summary>
/// This event is raised on an entity from which a disease is trying to spread just before it attempts to do so.
/// </summary>
[ByRefEvent]
public record DiseaseOutgoingSpreadAttemptEvent(float Power, float Chance, ProtoId<DiseaseSpreadPrototype> Type) : DiseaseSpreadAttemptEvent(Power, Chance, Type);

/// <summary>
/// This event is raised on an entity to which a disease is trying to spread just before it attempts to do so.
/// </summary>
[ByRefEvent]
public record DiseaseIncomingSpreadAttemptEvent(float Power, float Chance, ProtoId<DiseaseSpreadPrototype> Type) : DiseaseSpreadAttemptEvent(Power, Chance, Type);
