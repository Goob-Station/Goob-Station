using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class FleshPassiveComponent : Component
{
    [DataField, NonSerialized]
    public List<EntityUid> FleshMimics = new();

    [DataField]
    public int MaxMimics = 10;

    [DataField]
    public float MimicHealMultiplier = 5f;

    [DataField, NonSerialized]
    public EntityUid? FleshStomach;

    [DataField]
    public float BaseMoveSpeedPerFlesh = 0.0003f;

    [DataField]
    public float BaseAttackRatePerFlesh = 0.002f;

    [DataField]
    public float BaseHealingPerFlesh = 0.0015f;

    [DataField]
    public float OrganMultiplier = 2f;

    [DataField]
    public float MeatMultiplier = 1.1f;

    [DataField]
    public float BodyPartMultiplier = 5f;

    [DataField]
    public float MobMultiplier = 5f;

    [DataField]
    public float BrainMultiplier = 2f;

    [DataField]
    public float HumanMultiplier = 2f;

    [DataField]
    public float AscensionMultiplier = 2f;

    [DataField]
    public float PainHealMultiplier = 5f;

    [DataField]
    public float BoneHealMultiplier = 10f;

    [DataField]
    public float WoundHealMultiplier = 10f;

    [DataField]
    public float BloodHealMultiplier = 20f;

    [DataField]
    public float BleedReductionMultiplier = 5f;

    [DataField]
    public ProtoId<TagPrototype> MeatTag = "Meat";

    [DataField]
    public float HealInterval = 1f;

    [ViewVariables]
    public float Accumulator;

    [DataField]
    public FixedPoint2 TrackedDamage;

    [DataField]
    public FixedPoint2 MimicDamage = 10;
}
