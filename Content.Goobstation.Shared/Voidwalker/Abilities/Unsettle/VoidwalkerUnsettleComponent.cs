using Content.Goobstation.Shared.SpecialAnimation;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Voidwalker.Abilities.Unsettle;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class VoidwalkerUnsettleComponent : Component
{
    [DataField]
    public EntProtoId UnsettleAction = "ActionVoidwalkerUnsettle";

    [DataField, AutoNetworkedField]
    public EntityUid? UnsettleActionEntity;

    /// <summary>
    /// The ID of the scream prototype.
    /// </summary>
    [DataField]
    public ProtoId<EmotePrototype> ScreamProtoId = "Scream";

    /// <summary>
    /// How long you need to stare at someone to stun em
    /// </summary>
    [DataField]
    public TimeSpan UnsettleDoAfterDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The duration the creature is stunned for if successful.
    /// </summary>
    [DataField]
    public TimeSpan UnsettleStunDuration= TimeSpan.FromSeconds(4);

    /// <summary>
    /// The amount of stamina damage the target takes.
    /// </summary>
    [DataField]
    public float UnsettleStaminaDamage = 80f;

    /// <summary>
    /// The ID of the doafter.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public ushort? UnsettleDoAfterId;

    /// <summary>
    /// The sprite flashed on the targets screen during a jumpscare.
    /// </summary>
    [DataField]
    public SpriteSpecifier JumpscareSprite = new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Mobs/Voidwalker/voidwalker.rsi"), "voidwalker");

    /// <summary>
    /// The prototype of how the sprite is animated during the jumpscare.
    /// </summary>
    [DataField]
    public ProtoId<SpecialAnimationPrototype>? JumpscarePrototype;

    /// <summary>
    /// The sound played during a jumpscare for the target.
    /// </summary>
    [DataField]
    public SoundSpecifier JumpscareSound;
}
