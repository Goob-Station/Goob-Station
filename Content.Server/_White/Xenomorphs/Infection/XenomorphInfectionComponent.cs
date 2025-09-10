using Content.Shared._White.Xenomorphs.Infection;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server._White.Xenomorphs.Infection;

[RegisterComponent]
public sealed partial class XenomorphInfectionComponent : SharedXenomorphInfectionComponent
{
    [DataField]
    public int MaxGrowthStage = 1;

    [DataField]
    public EntProtoId LarvaPrototype = "MobXenomorphLarva";

    /// <summary>
    /// The probability of infection growth per GrowTime.
    /// </summary>
    [DataField]
    public float GrowProb = 1f;

    /// <summary>
    /// The time required for infection to grow.
    /// </summary>
    [DataField]
    public TimeSpan GrowTime = TimeSpan.FromSeconds(25);

    [DataField]
    public Dictionary<int, List<EntityEffect>> Effects = new ();

    [ViewVariables]
    public TimeSpan NextPointsAt;

    [ViewVariables]
    public EntityUid? Infected;

}
