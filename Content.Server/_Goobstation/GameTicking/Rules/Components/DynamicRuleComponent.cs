using Content.Shared.Dataset;
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
    [ViewVariables(VVAccess.ReadOnly)] public float ThreatLevel = 0f;

    /// <summary>
    ///     Used for EORG display.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)] public float RoundstartBudget = 0f;

    /// <summary>
    ///     Used for EORG display.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)] public float MidroundBudget = 0f;

    #endregion

    #region Gamerules

    [DataField] public ProtoId<DatasetPrototype> RoundstartRulesPool;

    /// <summary>
    ///     Used for EORG.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)] public List<(EntProtoId, EntityUid?)> ExecutedRules = new();

    #endregion
}
