using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Alert;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SpecialPassives.BoostedImmunity.Components;

/// <summary>
///     Entities with this will rapidly heal non-physical damage. This component holds all the relevant data.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BoostedImmunityComponent : Component
{
    /// <summary>
    /// The alert id of the component (if one should exist)
    /// </summary>
    public ProtoId<AlertPrototype>? AlertId;

    /// <summary>
    /// How long should the effect go on for?
    /// </summary>
    [DataField]
    public float? Duration;

    public TimeSpan MaxDuration = TimeSpan.Zero;

    public TimeSpan UpdateTimer = default!;

    /// <summary>
    /// Delay between healing ticks.
    /// </summary>
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Current mobstate of the entity.
    /// </summary>
    public MobState Mobstate;

    /// <summary>
    /// Should the ability continue while on fire?
    /// </summary>
    [DataField]
    public bool IgnoreFire = false;

    /// <summary>
    /// Should the ability continue while dead?
    /// </summary>
    [DataField]
    public bool WorkWhileDead = false;

    /// <summary>
    /// Should the entity be rid of all disabilities?
    /// </summary>
    [DataField]
    public bool RemoveDisabilities = true;

    /// <summary>
    /// Should chemicals be cleansed from the bloodstream?
    /// </summary>
    [DataField]
    public bool CleanseChemicals = true;

    [DataField]
    public FixedPoint2 CleanseChemicalsAmount = 25;

    /// <summary>
    /// Should the entity be sobered?
    /// </summary>
    [DataField]
    public bool ApplySober = true;

    /// <summary>
    /// Should the entity resist vomiting?
    /// </summary>
    [DataField]
    public bool ResistNausea = true;

    /// <summary>
    /// Should the entity be cleared of pacifism?
    /// </summary>
    [DataField]
    public bool RemovePacifism = true;

    /// <summary>
    /// Should the entity have any present alien embryos removed and destroyed?
    /// </summary>
    [DataField]
    public bool RemoveAlienEmbryo = true;

    /// <summary>
    /// Should the entity be cured of all diseases?
    /// </summary>
    [DataField]
    public bool RemoveDiseases = true;

    [DataField]
    public float ToxinHeal = -10f;

    [DataField]
    public float CellularHeal = -10f;

    [DataField]
    public int EyeDamageHeal = 1;

    // add bools later for curing diseases and mutations (when they exist)
}
