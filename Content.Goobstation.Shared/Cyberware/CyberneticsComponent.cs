using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Cyberware;

/// <summary>
/// Marks a body part or organ as cybernetic.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CyberneticsComponent : Component
{
    /// <summary>
    /// What the cyberware menu calls this implant.
    /// Falls back to the entities name.
    /// Use this when the implant is a component added to something like a normal leg.
    /// </summary>
    [DataField]
    public LocId? Name;

    /// <summary>
    /// What the cyberware menu shows as its description.
    /// </summary>
    [DataField]
    public LocId? Description;

    /// <summary>
    /// Whether this implant has an effect that can be switched off from the cyberware menu.
    /// </summary>
    [DataField]
    public bool CanToggle;

    /// <summary>
    /// Whether the implant is powered.
    /// This is usually set through the cyberware ui.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    /// Decides whether the implant is using activeDrain vs drain.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Active;

    /// <summary>
    /// Decides if the implant is using overload drain.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Overloaded;

    /// <summary>
    /// This is low because it can stack with other implants in the users body.
    /// </summary>
    [DataField]
    public DamageSpecifier EmpDamage = new()
    {
        DamageDict = new() { { "Shock", 4 } },
    };

    /// <summary>
    /// Passive sanity drained per second while installed and enabled.
    /// </summary>
    [DataField]
    public float? Drain;

    /// <summary>
    /// Drain while active.
    /// </summary>
    [DataField]
    public float? ActiveDrain;

    /// <summary>
    /// Extra drain while overloaded.
    /// </summary>
    [DataField]
    public float? OverloadDrain;

    /// <summary>
    /// Lowest clock level this implant supports.
    /// Only set this if underclocking has an actual effect (I.E. sandevistan being underclocked
    /// would mean it's slower but has less of a sanity drain).
    /// </summary>
    [DataField]
    public int MinClock;

    /// <summary>
    /// Highest clock level this implant supports.
    /// Only set this if overclocking has an actual effect (I.E. sandevistan being overclocked
    /// would mean it's faster but has an increased sanity drain).
    /// </summary>
    [DataField]
    public int MaxClock;

    /// <summary>
    /// Current clock level.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int ClockStep;

    /// <summary>
    /// Sanity drain multiplier for each clock level.
    /// </summary>
    [DataField]
    public Dictionary<int, float> ClockDrainMultipliers = new()
    {
        [-2] = 0.35f,
        [-1] = 0.7f,
        [0] = 1f,
        [1] = 1.8f,
        [2] = 3f,
    };

    /// <summary>
    /// Turns off the implant while set. Used for things like emp's disabling it.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan? RebootUntil;

    /// <summary>
    /// How far should the node be in the minigame?
    /// </summary>
    [DataField]
    public int PowerOffDepth = 2;

    /// <summary>
    /// How far should the node be in the minigame?
    /// </summary>
    [DataField]
    public int PowerOnDepth = 3;

    /// <summary>
    /// How far should the node be in the minigame?
    /// </summary>
    [DataField]
    public int ClockBaseDepth = 2;

    /// <summary>
    /// Unique description for overclock nodes. Using this is recommended.
    /// </summary>
    [DataField]
    public LocId? OverclockDesc;

    /// <summary>
    /// Unique description for underclock nodes. Using this is recommended.
    /// </summary>
    [DataField]
    public LocId? UnderclockDesc;
}

/// <summary>
/// Used mostly for the ui to communicate to the implants.
/// </summary>
[ByRefEvent]
public readonly record struct CyberwareChangedEvent(EntityUid Body);
