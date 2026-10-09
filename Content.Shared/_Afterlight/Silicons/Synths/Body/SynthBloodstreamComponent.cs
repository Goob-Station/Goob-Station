using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Afterlight.Silicons.Synths.Body;

[RegisterComponent, NetworkedComponent]
public sealed partial class SynthBloodstreamComponent : Component
{
    [DataField]
    public float MinHunger = 50f;

    [DataField]
    public float MinBloodLevel = 0.3f;

    [DataField]
    public float FullEfficiencyBloodLevel = 0.9f;

    [DataField]
    public float MinBloodEfficiency = 0.1f;

    [DataField]
    public float HungerCostPerRepair = 1.5f;

    [DataField]
    public float HungerCostPerBleed = 1f;

    [DataField]
    public SynthBloodstreamDamageSpecifier Damage = new();

    [DataField]
    public float BleedReductionAmount = 0.1f;

    [DataField]
    public SynthBloodstreamRegeneration BloodRegeneration = new();

    [DataField]
    public TimeSpan UpdateRate = TimeSpan.FromSeconds(1);

    [ViewVariables]
    public TimeSpan NextUpdate;

    [ViewVariables]
    public DamageSpecifier PassiveRepair = new();
}

[DataDefinition]
public sealed partial class SynthBloodstreamDamageSpecifier
{
    [DataField]
    public Dictionary<ProtoId<DamageGroupPrototype>, FixedPoint2> Groups = new();

    [DataField]
    public Dictionary<ProtoId<DamageTypePrototype>, FixedPoint2> Types = new();
}

[DataDefinition]
public sealed partial class SynthBloodstreamRegeneration
{
    [DataField]
    public float TargetBloodLevel = 1f;

    [DataField]
    public FixedPoint2 BloodRefreshAmount = FixedPoint2.New(0.03f);

    [DataField]
    public float HungerCostPerUnit = 0.5f;
}
