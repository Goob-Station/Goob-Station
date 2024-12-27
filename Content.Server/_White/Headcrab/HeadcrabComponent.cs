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
    [DataField(required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string JumpAction = "ActionHeadcrabJump";

    [DataField]
    public float ParalyzeTime = 3f;

    [DataField]
    public int ChancePounce = 33;

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
