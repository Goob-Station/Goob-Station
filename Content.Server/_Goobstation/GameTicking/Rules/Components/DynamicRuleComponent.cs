using Content.Shared.Dataset;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent]
public sealed partial class DynamicRuleComponent : Component
{
    #region Budgets

    /// <summary>
    ///     Ignore major threats and stack one upon each other :trollface:
    ///     Use this if chaos is your thing or you want a budget AAO
    /// </summary>
    [DataField] public bool Unforgiving = false;

    /// <summary>
    ///     Max threat available on lowpop
    /// </summary>
    [DataField] public float LowpopMaxThreat = 40f;

    /// <summary>
    ///     Maximum amount of threat available
    /// </summary>
    [DataField] public float MaxThreat = 100f;

    /// <summary>
    ///     
    /// </summary>
    public float ThreatLevel = 0f;

    /// <summary>
    ///     Used for EORG display.
    /// </summary>
    public float RoundstartBudget = 0f;

    /// <summary>
    ///     Used for EORG display.
    /// </summary>
    public float MidroundBudget = 0f;

    /// <summary>
    ///     Used for midround rolling.
    /// </summary>
    public float MidroundBudgetLeft = 0f;

    #endregion

    #region Gamerules

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

    #endregion

    #region Midround Clock

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

    #endregion

    #region Calculations

    /// <summary>
    ///     
    /// </summary>
    [DataField] public float ThreatCurveCentre = 0f;

    /// <summary>
    ///     
    /// </summary>
    public float ThreatCurveWidth = 1.8f;

    #endregion
}
