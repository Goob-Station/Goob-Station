using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent]
public sealed partial class DynamicRuleComponent : Component
{
    /// <summary>
    ///     
    /// </summary>
    public float ThreatLevel = 0f;

    /// <summary>
    ///     
    /// </summary>
    [DataField] public float MaxThreat = 100f;

    /// <summary>
    ///     
    /// </summary>
    [DataField] public float RoundstartBudget = 0f;

    /// <summary>
    ///     
    /// </summary>
    [DataField] public float MidroundBudget = 0f;

    [DataField] public ProtoId<DatasetPrototype>? RoundstartRulesPool = null;
    [DataField] public ProtoId<DatasetPrototype>? MidroundRulesPool = null;

    /// <summary>
    ///     Midround rules pool for rolling antag related events.
    /// </summary>
    public List<EntProtoId> MidroundRules = new();

    /// <summary>
    ///     Used for EORG.
    /// </summary>
    public List<EntProtoId> ExecutedRules = new();

    /// <summary>
    ///     How much time it takes in seconds for an antag event to be raised. (min)
    /// </summary>
    /// <remarks>Default is 10 minutes</remarks>
    [DataField] public float MidroundLowerBound = 600f;

    /// <summary>
    ///     How much time it takes in seconds for an antag event to be raised. (max)
    /// </summary>
    /// <remarks>Default is 20 minutes</remarks>
    [DataField] public float MidroundUpperBound = 1200f;

    /// <summary>
    ///     Clock for midround rolls.
    /// </summary>
    public float MidroundRollClock = 0f;
    /// <summary>
    ///     The first midround antag roll will happen 20 minutes into the shift.
    ///     After that it's all about random.
    /// </summary>
    public float MidroundRollTime = 1200f;

    /// <summary>
    ///     
    /// </summary>
    [DataField] public float ThreatPerMidroundRoll = 7f;

    /// <summary>
    ///     Max threat available on lowpop
    /// </summary>
    [DataField] public float LowpopMaxThreat = 40f;

    /// <summary>
    ///     Ignore major threats and stack one upon each other :trollface:
    ///     Use this if chaos is your thing or you want a budget AAO
    /// </summary>
    [DataField] public bool Unforgiving = false;
}
