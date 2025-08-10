using Content.Shared.Damage.Prototypes;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Werewolf.Components;

[RegisterComponent]
public sealed partial class WerewolfHowlComponent : Component
{
    /// <summary>
    /// Whether to apply the status effects to nearby entities, or just play the sounds.
    /// </summary>
    [DataField]
    public bool ApplyEffects = true;

    /// <summary>
    /// Whether the ability is active, or not.
    /// </summary>
    [DataField]
    public bool Active;

    [ViewVariables]
    public TimeSpan NextUpdate;

    /// <summary>
    /// How long the effects of the buffs of the ability will last on the entity
    /// </summary>
    [DataField]
    public TimeSpan HowlBuffsDuration = TimeSpan.FromSeconds(5f);

    /// <summary>
    ///  The flashed status effect prototype
    /// </summary>
    [ViewVariables]
    public ProtoId<StatusEffectPrototype> Flashed = "Flashed";

    /// <summary>
    ///  The slowed down status effect prototype
    /// </summary>
    [ViewVariables]
    public ProtoId<StatusEffectPrototype> SlowedDown = "SlowedDown";

    /// <summary>
    ///  The duration of the flashed effect
    /// </summary>
    [DataField]
    public TimeSpan FlashDuration = TimeSpan.FromSeconds(4);

    /// <summary>
    ///  The duration of the slowed down effect
    /// </summary>
    [DataField]
    public TimeSpan SlowedDownDuration = TimeSpan.FromSeconds(6);

    /// <summary>
    ///  The range that the entity lookup works with
    /// </summary>
    [DataField]
    public float Range = 5f;

    /// <summary>
    ///  The max range of the howl sound effect
    /// </summary>
    [DataField]
    public int HowlSoundMaxRange = 20;

    [DataField]
    public SoundSpecifier? HowlSoundNear = new SoundPathSpecifier("/Audio/_DV/Voice/Vulpkanin/howl.ogg"); // todo: change

    [DataField]
    public SoundSpecifier? HowlSoundFar = new SoundPathSpecifier("/Audio/_DV/Voice/Vulpkanin/howl.ogg") // todo: change
    {
        Params = AudioParams.Default.WithVolume(-5).WithPitchScale(0.45f),
    };

    /// <summary>
    ///  The damage reduction to apply.
    /// </summary>
    [DataField]
    public ProtoId<DamageModifierSetPrototype>? NewDamageModifier;

    /// <summary>
    ///  The old damage modifier to apply once the effect is over.
    /// </summary>
    [DataField]
    public ProtoId<DamageModifierSetPrototype>? OldDamageModifier;

    /// <summary>
    ///  The walk speed of the werewolf once the howl effect starts
    /// </summary>
    [DataField]
    public float WalkSpeed = 1.5f;

    /// <summary>
    ///  The run speed of the werewolf once the howl effect starts
    /// </summary>
    [DataField]
    public float RunSpeed = 1.5f;
}
