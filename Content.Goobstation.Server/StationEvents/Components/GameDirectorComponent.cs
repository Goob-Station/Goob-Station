// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.StationEvents.SecretPlus;
using Content.Goobstation.Server.StationEvents.Metric;
using Content.Shared.Random;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(GameDirector.GameDirectorSystem))]
public sealed partial class GameDirectorComponent : Component
{

    /// <summary>
    ///   How long until the next check for an event runs
    ///   Default value is how long until first event is allowed
    /// </summary>
    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan TimeNextEvent;

    /// <summary>
    ///   When the current beat started
    /// </summary>
    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan BeatStart;

    /// <summary>
    ///   The chaos we measured last time we ran
    ///   This is helpful for ViewVariables and perhaps as a cache to hold chaos for other functions to use.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ChaosMetrics CurrentChaos = new();

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ChaosMetrics BeatStartChaos = new();

    /// <summary>
    ///   The story we are currently executing from stories (for easier debugging). Since it is for
    ///   debugging, it does not need a DataField.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<StoryPrototype> CurrentStoryName;

    [ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<StoryBeatPrototype>? CurrentBeat;

    /// <summary>
    ///   Remaining beats in the story we are currently executing (a list of beat IDs)
    /// </summary>
    [DataField]
    public List<ProtoId<StoryBeatPrototype>> RemainingBeats = new();

    /// <summary>
    /// Does this round start with multiple antags.
    /// </summary>
    [DataField]
    public bool DualAntags;

    /// <summary>
    /// Does this round start with antags at all?.
    /// </summary>
    [DataField]
    public bool NoRoundstartAntags;

    /// <summary>
    ///   Which stories the director can choose from (so we can change flavor of director by loading different stories)
    ///   One of these get picked randomly each time the current story is exhausted.
    /// </summary>
    [DataField]
    public ProtoId<StoryPrototype>[]? Stories;

    /// <summary>
    ///   A beat name we always use when we cannot find any stories to use.
    /// </summary>
    [DataField]
    public ProtoId<StoryBeatPrototype> FallbackBeatName = "Peace";

    /// <summary>
    ///   All the events that are allowed to run in the current story.
    /// </summary>
    [DataField]
    public List<PossibleEvent> PossibleEvents = new();

    [ViewVariables]
    public List<FiredEvent> FiredEvents = new();

    [DataField]
    public TimeSpan PendingDuration = TimeSpan.FromMinutes(10);

    [DataField]
    public TimeSpan RepeatWindow = TimeSpan.FromMinutes(20);

    [DataField]
    public float RepeatFactor = 0.2f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float DestructionBudget;

    [DataField]
    public float MaxDestructionBudget = 3f;

    [DataField]
    public float DestructionRegenPerMinute = 0.1f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan LastBudgetUpdate;

    [DataField]
    public int ReferencePlayers = 50;

    [DataField]
    public float MinPopulationScale = 0.5f;

    [DataField]
    public float MaxPopulationScale = 1.5f;

    [DataField]
    public HashSet<ChaosMetric> PopulationScaledMetrics = new()
    {
        ChaosMetric.Hostile,
        ChaosMetric.Friend,
        ChaosMetric.Combat,
        ChaosMetric.Death,
        ChaosMetric.Medical,
        ChaosMetric.Security,
    };

    [DataField]
    public ProtoId<StoryBeatPrototype>? DenouementBeat = "Denouement";

    [DataField]
    public Dictionary<ChaosMetric, ProtoId<DepartmentPrototype>> Responders = new()
    {
        { ChaosMetric.Atmos, "Engineering" },
        { ChaosMetric.Power, "Engineering" },
        { ChaosMetric.Hostile, "Security" },
        { ChaosMetric.Anomaly, "Science" },
    };

    [DataField]
    public int MinResponders = 1;

    [DataField]
    public TimeSpan ReinforcementCooldown = TimeSpan.FromMinutes(15);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan LastReinforcement;

    [ViewVariables]
    public List<EntProtoId> RoundstartAntags = new();

    [DataField]
    public TimeSpan NoEventRetryDelay = TimeSpan.FromSeconds(60);

    /// <summary>
    /// All the possible roundstart antags.
    /// </summary>
    [DataField]
    public ProtoId<WeightedRandomPrototype> RoundStartAntagsWeightTable = "GameDirector";
}

public readonly record struct FiredEvent(EntProtoId Event, TimeSpan Time, ChaosMetrics Chaos);

/// <summary>
///   Caches a possible StationEvent prototype with the chaos expected (from the game rule's data)
///   A list of PossibleEvents are built and cached by the game director.
/// </summary>
[DataDefinition]
public sealed partial class PossibleEvent
{
    /// <summary>
    ///   ID of a station event prototype (anomaly, spiders, pizzas, etc) that could occur
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId StationEvent;

    /// <summary>
    ///   Expected Chaos changes when this event occurs.
    ///   Used by the GameDirector, which picks an event expected to make the desired chaos changes.
    ///   Copy of the StationEventComponent.Chaos field from the relevant event.
    /// </summary>
    [DataField]
    public ChaosMetrics Chaos = new();

    [DataField]
    public float Weight = 1f;

    [DataField]
    public int Destruction;

    [DataField]
    public int RequiredGhosts;

    [DataField]
    public ProtoId<EventTypePrototype> EventType = "Neutral";

    [DataField]
    public HashSet<ProtoId<DepartmentPrototype>> Reinforces = new();

    [DataField]
    public int UnderstaffedBelow;

    [DataField]
    public HashSet<ProtoId<DepartmentPrototype>> RequiresStaff = new();

    public PossibleEvent()
    {
    }

    public PossibleEvent(EntProtoId stationEvent,
        ChaosMetrics chaos,
        float weight,
        int destruction,
        int requiredGhosts,
        ProtoId<EventTypePrototype> eventType)
    {
        StationEvent = stationEvent;
        Chaos = chaos;
        Weight = weight;
        Destruction = destruction;
        RequiredGhosts = requiredGhosts;
        EventType = eventType;
    }
}
