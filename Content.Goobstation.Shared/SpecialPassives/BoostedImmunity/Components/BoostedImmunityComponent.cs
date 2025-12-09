using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Alert;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.SpecialPassives.BoostedImmunity.Components;

/// <summary>
///     Entities with this will rapidly heal non-physical damage. This component holds all the relevant data.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class BoostedImmunityComponent : Component
{
    /// <summary>
    /// The alert id of the component (if one should exist)
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype>? AlertId;

    /// <summary>
    /// How long should the effect go on for?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float? Duration;

    [DataField, AutoNetworkedField]
    public TimeSpan MaxDuration = TimeSpan.Zero;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan UpdateTimer = default!;

    /// <summary>
    /// Delay between healing ticks.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Current mobstate of the entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public MobState Mobstate;

    /// <summary>
    /// Should the ability continue while on fire?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IgnoreFire = false;

    /// <summary>
    /// Should the ability continue while dead?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool WorkWhileDead = false;

    /// <summary>
    /// Should the entity be rid of all disabilities?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RemoveDisabilities = true;

    /// <summary>
    /// Should chemicals be cleansed from the bloodstream?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool CleanseChemicals = true;

    [DataField, AutoNetworkedField]
    public FixedPoint2 CleanseChemicalsAmount = 25;

    /// <summary>
    /// Should the entity be sobered?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ApplySober = true;

    /// <summary>
    /// Should the entity resist vomiting?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ResistNausea = true;

    /// <summary>
    /// Should the entity be cleared of pacifism?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RemovePacifism = true;

    /// <summary>
    /// Should the entity have any present alien embryos removed and destroyed?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RemoveAlienEmbryo = true;

    [DataField, AutoNetworkedField]
    public float ToxinHeal = -10f;

    [DataField, AutoNetworkedField]
    public float CellularHeal = -10f;

    [DataField, AutoNetworkedField]
    public int EyeDamageHeal = 1;

    // add bools later for curing diseases and mutations (when they exist)
}
