using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Disease;

/// <summary>
/// This event is raised on diseases on update.
/// </summary>
[ByRefEvent]
public record struct DiseaseUpdateEvent(Entity<DiseaseCarrierComponent> Ent);

/// <summary>
/// This event is raised on disease effects when triggered.
/// </summary>
[ByRefEvent]
public record struct DiseaseEffectEvent(DiseaseEffectComponent Comp, Entity<DiseaseComponent> Disease, Entity<DiseaseCarrierComponent> Ent);

/// <summary>
/// This event is raised on disease effects when triggered but conditions have failed.
/// </summary>
[ByRefEvent]
public record struct DiseaseEffectFailedEvent(DiseaseEffectComponent Comp, Entity<DiseaseComponent> Disease, Entity<DiseaseCarrierComponent> Ent);

/// <summary>
/// This event is raised on entities that got a new disease.
/// </summary>
[ByRefEvent]
public record struct DiseaseGainedEvent(Entity<DiseaseComponent> Disease);

/// <summary>
/// This event is raised on entities which just lost a disease.
/// </summary>
[ByRefEvent]
public record struct DiseaseCuredEvent(Entity<DiseaseComponent> Disease);

/// <summary>
/// Raised on a newly created base disease entity to clone the provided entity onto it.
/// </summary>
[ByRefEvent]
public record struct DiseaseCloneEvent(Entity<DiseaseComponent> Source);

/// <summary>
/// This event is raised on an entity just before it's infected. Set <see cref="CanInfect"/> to false to prevent the infection.
/// </summary>
[ByRefEvent]
public record struct DiseaseInfectAttemptEvent(Entity<DiseaseComponent> Disease, bool CanInfect = true);

/// <summary>
/// This event is raised on a disease effect just before it's triggered to check whether the effect should be triggered.
/// </summary>
[ByRefEvent]
public record struct DiseaseCheckConditionsEvent(DiseaseEffectComponent Comp, Entity<DiseaseComponent> Disease, Entity<DiseaseCarrierComponent> Ent, bool DoEffect = true);

/// <summary>
/// This event is raised on a diseased entity to get its immune resistance.
/// </summary>
[ByRefEvent]
public record struct GetImmunityEvent(Entity<DiseaseComponent> Disease, float ImmunityGainRate = 0f, float ImmunityStrength = 0f);

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
