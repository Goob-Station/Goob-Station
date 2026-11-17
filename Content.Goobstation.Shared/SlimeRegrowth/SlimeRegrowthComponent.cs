using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Actions;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SlimeRegrowth;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SlimeRegrowthSystem))]
public sealed partial class SlimeRegrowthComponent : Component
{
    [DataField]
    public EntProtoId Action = "ActionSlimeRegrowLimb";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(10);

    [DataField]
    public float HungerCost = 50f;

    [DataField]
    public float MinHunger = 50f;

    [DataField]
    public FixedPoint2 BloodCost = 30;

    [DataField]
    public float MinBloodLevel = 0.8f;

    [DataField]
    public DamageSpecifier? Damage;

    [DataField]
    public List<BodyPartType> Parts = new()
    {
        BodyPartType.Arm,
        BodyPartType.Hand,
        BodyPartType.Leg,
        BodyPartType.Foot,
    };

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Voice/Slime/slime_squish.ogg");
}

public sealed partial class SlimeRegrowLimbActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class SlimeRegrowLimbDoAfterEvent : SimpleDoAfterEvent;
