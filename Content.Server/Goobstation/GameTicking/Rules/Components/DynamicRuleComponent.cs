using Content.Shared.Dataset;
using Content.Shared.GameTicking.Components;
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
    ///     
    /// </summary>
    public List<EntProtoId> MidroundRules = new();

    /// <summary>
    ///     Used for EORG
    /// </summary>
    public List<EntProtoId> ExecutedRules = new();

    /// <summary>
    ///     
    /// </summary>
    [DataField] public TimeSpan MidroundLowerBound = TimeSpan.FromMinutes(10);

    /// <summary>
    ///     
    /// </summary>
    [DataField] public TimeSpan MidroundUpperBound = TimeSpan.FromMinutes(60);

    /// <summary>
    ///     
    /// </summary>
    [DataField] public float ThreatPerMidroundRoll = 7f;

    /// <summary>
    ///     
    /// </summary>
    [DataField] public float ThreatCurveCentre = 0f;

    /// <summary>
    ///     
    /// </summary>
    public float ThreatCurveWidth = 1.8f;

    /// <summary>
    ///     Max threat available on lowpop
    /// </summary>
    [DataField] public float LowpopMaxThreat = 40f;
}
