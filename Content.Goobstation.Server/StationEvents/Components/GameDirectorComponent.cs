// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.StationEvents.SecretPlus;
using Content.Goobstation.Server.StationEvents.Metric;
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
    ///   Which stories the director can choose from (so we can change flavor of director by loading different stories)
    ///   One of these get picked randomly each time the current story is exhausted.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<StoryPrototype>, float> Stories = new();

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

    [DataField]
    public TimeSpan HelpCooldown = TimeSpan.FromMinutes(12);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan LastHelp;

    [DataField]
    public TimeSpan NoEventRetryDelay = TimeSpan.FromSeconds(60);

    [DataField]
    public float ThreatPerPlayer = 0.35f;

    [DataField]
    public float RoundstartThreatScale = 1f;

    [DataField]
    public float MinRoundstartThreat = 2f;

    [DataField]
    public ProtoId<AntagThreatPrototype> ThreatTable = "Default";

    [DataField]
    public float BaseThreatCap = 4f;

    [DataField]
    public float ThreatCapPerSecurity = 6f;

    [DataField]
    public float MinThreatPurchase = 1.5f;

    [DataField]
    public TimeSpan PendingThreatTime = TimeSpan.FromMinutes(5);

    [DataField]
    public TimeSpan RoundstartPendingTime = TimeSpan.FromMinutes(20);

    [DataField]
    public float CountStretch = 2f;

    [DataField]
    public float OutOfRangePenalty = 0.5f;

    [ViewVariables]
    public List<ThreatPurchase> Purchases = new();

    [ViewVariables]
    public int RoundstartPopulation;

    [DataField]
    public Dictionary<ChaosMetric, float> MetricScales = new()
    {
        { ChaosMetric.Hostile, 200f },
        { ChaosMetric.Friend, 500f },
        { ChaosMetric.Combat, 500f },
        { ChaosMetric.Death, 70f },
        { ChaosMetric.Medical, 190f },
        { ChaosMetric.Security, 130f },
        { ChaosMetric.Atmos, 540f },
        { ChaosMetric.Fire, 20f },
        { ChaosMetric.Power, 150f },
        { ChaosMetric.Infestation, 75f },
        { ChaosMetric.Mess, 1550f },
        { ChaosMetric.Anomaly, 90f },
        { ChaosMetric.Hunger, 80f },
        { ChaosMetric.Thirst, 75f },
        { ChaosMetric.Charge, 50f },
    };

    [DataField]
    public float OvershootPenalty = 2.5f;

    [DataField]
    public TimeSpan SettleTime = TimeSpan.FromMinutes(3);

    [DataField]
    public TimeSpan EffectDelay = TimeSpan.FromMinutes(10);

    [DataField]
    public HashSet<ChaosMetric> RelaxMetrics = new()
    {
        ChaosMetric.Atmos,
        ChaosMetric.Fire,
        ChaosMetric.Power,
        ChaosMetric.Medical,
        ChaosMetric.Death,
        ChaosMetric.Security,
    };

    [DataField]
    public float RelaxStrain = 2f;

    [DataField]
    public TimeSpan RelaxTime = TimeSpan.FromMinutes(6);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan RelaxUntil;

    [DataField]
    public Dictionary<ChaosMetric, ProtoId<DepartmentPrototype>> SpotlightDepartments = new()
    {
        { ChaosMetric.Atmos, "Engineering" },
        { ChaosMetric.Fire, "Engineering" },
        { ChaosMetric.Power, "Engineering" },
        { ChaosMetric.Hostile, "Security" },
        { ChaosMetric.Security, "Security" },
        { ChaosMetric.Anomaly, "Science" },
        { ChaosMetric.Medical, "Medical" },
        { ChaosMetric.Death, "Medical" },
        { ChaosMetric.Infestation, "Civilian" },
        { ChaosMetric.Mess, "Civilian" },
        { ChaosMetric.Hunger, "Civilian" },
        { ChaosMetric.Thirst, "Civilian" },
    };

    [DataField]
    public TimeSpan SpotlightWindow = TimeSpan.FromMinutes(20);

    [DataField]
    public float SpotlightBonus = 1f;

    [ViewVariables]
    public Dictionary<ProtoId<DepartmentPrototype>, TimeSpan> LastSpotlight = new();

    [DataField]
    public int LowPopPlayers = 20;

    [DataField]
    public float MaxLowPopDelayScale = 2.5f;

    [DataField]
    public List<EntProtoId> ArmedResponses = new()
    {
        "UnknownShuttleNTIncorporation",
        "SpawnERTSecurity",
    };

    [DataField]
    public float ArmedResponseThreshold = 150f;

    [DataField]
    public TimeSpan ArmedResponseDelay = TimeSpan.FromMinutes(5);

    [DataField]
    public TimeSpan ArmedResponseCooldown = TimeSpan.FromMinutes(20);

    [DataField]
    public int MaxArmedResponses = 2;

    [DataField]
    public int ArmedResponseGhosts = 3;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan OverwhelmedSince;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan LastArmedResponse;

    [ViewVariables]
    public int ArmedResponseCount;

    [DataField]
    public TimeSpan SpawnerLifetime = TimeSpan.FromMinutes(20);
}

public sealed class FiredEvent
{
    public EntProtoId Event;
    public EntityUid? Rule;
    public TimeSpan Time;
    public ChaosMetrics Chaos = new();
    public ChaosMetrics Before = new();
    public bool EffectLogged;
}

public sealed class ThreatPurchase
{
    public EntityUid Rule;
    public EntProtoId Proto;
    public float Threat;
    public int Count;
    public int MinCount;
    public int MaxCount;
    public TimeSpan Time;
    public bool Roundstart;
    public bool GhostRole;
    public bool SpawnersCleared;
}

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
