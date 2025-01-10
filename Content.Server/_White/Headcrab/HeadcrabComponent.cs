using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._White.Headcrab;

[Access(typeof(HeadcrabSystem))]
[RegisterComponent]
public sealed partial class HeadcrabComponent : Component
{
    /// <summary>
    /// WorldTargetAction
    /// </summary>
    [DataField]
    public EntProtoId JumpAction = "ActionHeadcrabJump";

    [DataField]
    public TimeSpan ParalyzeTime = TimeSpan.FromSeconds(3);

    [DataField]
    public float ChancePounce = 0.33f;

    [DataField(required: true)]
    public DamageSpecifier Damage = default!;

    public EntityUid EquippedOn;

    [ViewVariables]
    public float Accumulator = 0;

    [DataField]
    public float DamageFrequency = 5;

    [DataField]
    public SoundSpecifier? JumpSound = new SoundPathSpecifier("/Audio/_White/Misc/Headcrab/headcrab_jump.ogg");

}
