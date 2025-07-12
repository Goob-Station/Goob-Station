using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Server.Research.Oracle;

[RegisterComponent]
public sealed partial class OracleComponent : Component
{
    public const string SolutionName = "fountain";

    [DataField(required: true)]
    public ProtoId<WeightedRandomPrototype> DemandTypes;

    [DataField]
    public List<EntProtoId> BlacklistedDemands = new();

    [DataField(required: true)]
    public List<ProtoId<WeightedRandomEntityPrototype>> RewardEntities;

    [DataField(required: true)]
    public ProtoId<WeightedRandomPrototype> RewardReagents;

    /// <summary>
    ///     The chance to dispense a completely random chemical instead of what's listed in <see cref="RewardReagents"/>
    /// </summary>
    [DataField]
    public float AbnormalReagentChance = 0.2f;

    [DataField]
    public TimeSpan
        NextDemandTime = TimeSpan.Zero,
        NextBarkTime = TimeSpan.Zero,
        NextRejectTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan
        DemandDelay = TimeSpan.FromMinutes(10),
        BarkDelay = TimeSpan.FromMinutes(2),
        RejectDelay = TimeSpan.FromSeconds(10);

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityPrototype DesiredPrototype = default!;

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityPrototype? LastDesiredPrototype = default!;

    [DataField("demandMessages")]
    public IReadOnlyList<string> DemandMessages = new[]
    {
        "oracle-demand-1",
        "oracle-demand-2",
        "oracle-demand-3",
        "oracle-demand-4",
        "oracle-demand-5",
        "oracle-demand-6",
        "oracle-demand-7",
        "oracle-demand-8",
        "oracle-demand-9",
        "oracle-demand-10",
        "oracle-demand-11",
        "oracle-demand-12"
    };

    [DataField("rejectMessages")]
    public IReadOnlyList<string> RejectMessages = new[]
    {
        "ἄγνοια",
        "υλικό",
        "ἀγνωσία",
        "γήινος",
        "σάκλας"
    };
}
