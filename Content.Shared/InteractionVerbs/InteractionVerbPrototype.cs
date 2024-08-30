using Content.Shared.DoAfter;
using Content.Shared.InteractionVerbs.Events;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Content.Shared.InteractionVerbs;

/// <summary>
///     Represents an action that can be performed on an entity.
/// </summary>
[Prototype("Interaction"), Serializable]
public sealed partial class InteractionVerbPrototype : IPrototype, IInheritingPrototype
{
    /// <inheritdoc />
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<InteractionVerbPrototype>))]
    public string[]? Parents { get; }

    /// <inheritdoc />
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; }

    [IdDataField]
    public string ID { get; } = default!;

    // Locale getters
    public string Name => Loc.TryGetString($"interaction-{ID}-name", out var loc) ? loc : ID;

    public string? Description => Loc.TryGetString($"interaction-{ID}-description" , out var loc) ? loc : null;

    /// <summary>
    ///     Sprite of the icon that the user sees on the verb button.
    /// </summary>
    [DataField]
    public SpriteSpecifier? Icon;

    /// <summary>
    ///     Specifies what effects are shown when this verb is performed successfully, or unsuccessfully.
    ///     Effects specified here are shown after the associated do-after has ended, if any.
    /// </summary>
    [DataField]
    public EffectSpecifier? EffectSuccess, EffectFailure;

    /// <summary>
    ///     Specifies what popups are shown when a do-after for this verb is started.
    ///     This is only ever used if <see cref="Delay"/> is set to a non-zero value.
    /// </summary>
    [DataField]
    public EffectSpecifier? EffectDelayed;

    /// <summary>
    ///     The requirement of this verb.
    /// </summary>
    [DataField]
    public InteractionRequirement? Requirement = null;

    /// <summary>
    ///     The action of this verb. It defines the conditions under which this verb is shown, as well as what the verb does.
    /// </summary>
    /// <remarks>Made server-only because many actions require authoritative access to the server.</remarks>
    [DataField(serverOnly: true)]
    public InteractionAction? Action = null;

    /// <summary>
    ///     If true, this action will be hidden if the <see cref="Requirement"/> does not pass its IsMet check. Otherwise it will be shown, but disabled.
    /// </summary>
    /// <remarks>I apologize, I could not come up with a better name.</remarks>
    [DataField]
    public bool HideByRequirement = false;

    /// <summary>
    ///     If true, this action will be hidden if the <see cref="Action"/> does not pass its IsAllowed check. Otherwise it will be shown, but disabled.
    /// </summary>
    [DataField]
    public bool HideWhenInvalid = true;

    /// <summary>
    ///     The delay of the verb. Anything greater than zero constitutes a do-after.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.Zero;

    /// <summary>
    ///     Cooldown between uses of this verb. Applied per user or per user-target pair (see <see cref="GlobalCooldown"/>) and before the do-after.
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(0.5f);

    /// <summary>
    ///     If true, the contests defined in <see cref="AllowedContests"/> will affect the delay or the cooldown of the verb.
    /// </summary>
    [DataField]
    public bool ContestDelay = true, ContestCooldown = false;

    /// <summary>
    ///     If true, the cooldown of this verb will be applied regardless of the verb target,
    ///     i.e. a user won't be able to apply the same verb to any different entity until the cooldown ends.
    /// </summary>
    [DataField]
    public bool GlobalCooldown = false;

    /// <summary>
    ///     Arguments of the do-after shown if <see cref="Delay"/> is greater than zero.
    ///     The user, target, needHand, event, and other required parameters are set up automatically when the do-after is created.
    /// </summary>
    [DataField]
    public DoAfterArgs DoAfter = new DoAfterArgs()
    {
        User = EntityUid.Invalid,
        NetUser = NetEntity.Invalid,
        BreakOnDamage = true,
        BreakOnTargetMove = true,
        BreakOnUserMove = true,
        BreakOnWeightlessMove = true,
        RequireCanInteract = false,
        // Never used, but must be present because the field is non-nullable and will error during serialization if not set.
        Event = new InteractionVerbDoAfterEvent(default, default!)
    };

    [DataField]
    public RangeSpecifier Range = new();

    /// <summary>
    ///     Range of contest advantages valid for this verb.
    ///     If the user's contest advantage is outside of this range, the verb will be disabled or hidden.
    /// </summary>
    /// <remarks>If not specified, contest advantage won't be calculated until the verb is performed.</remarks>
    [DataField]
    public RangeSpecifier? ContestAdvantageRange;

    /// <summary>
    ///     Range of contest advantages that the user can gain while using this verb.
    ///     The user's advantage will never exceed this range. This is applied after <see cref="ContestAdvantageRange"/> is checked.
    /// </summary>
    /// <returns></returns>
    [DataField]
    public RangeSpecifier ContestAdvantageLimit = new() { Min = 0.2f, Max = 5f };

    [DataField]
    public ContestType AllowedContests = ContestType.None;

    /// <summary>
    ///     Whether this interaction implies direct body contact (transfer of fibers, fingerprints, etc).
    /// </summary>
    [DataField("contactInteraction")]
    public bool DoContactInteraction = true;

    [DataField]
    public bool RequiresHands = false;

    [DataField]
    public bool RequiresCanInteract = true;

    /// <summary>
    ///     If true, this verb can be invoked by the user on itself.
    /// </summary>
    [DataField]
    public bool AllowSelfInteract = false;

    /// <summary>
    ///     Priority of the verb. Verbs with higher priority will be shown first.
    /// </summary>
    [DataField]
    public int Priority = 0;

    /// <summary>
    ///     If true, this verb can be invoked on any entity that the action is allowed on, even if its components don't specify it.
    /// </summary>
    [DataField]
    public bool Global = false;

    [DataDefinition, Serializable]
    public partial struct RangeSpecifier()
    {
        [DataField] public float Min = 0f;
        [DataField] public float Max = float.PositiveInfinity;
        [DataField] public bool Inverse = false;

        public bool IsInRange(float value) => (Inverse ? value < Min || value > Max : value >= Min && value <= Max);

        public float Clamp(float value)
        {
            DebugTools.Assert(!Inverse, "Inverse ranges do not support clamping.");
            return Math.Clamp(value, Min, Max);
        }
    }

    [DataDefinition, Serializable]
    public partial class EffectSpecifier
    {
        [DataField]
        public EffectTargetSpecifier EffectTarget = EffectTargetSpecifier.TargetThenUser;

        /// <summary>
        ///     The interaction popup to show, at <see cref="EffectLocation"/>. If null, no popup will be shown.
        /// </summary>
        [DataField]
        public ProtoId<InteractionPopupPrototype>? Popup = null;

        /// <summary>
        ///     Sound played when the effect is shown, at <see cref="EffectLocation"/>. If null, no sound will be played.
        /// </summary>
        [DataField]
        public SoundSpecifier? Sound;

        /// <summary>
        ///     If true, the sound will be perceived by everyone in the PVS of the popup.
        ///     Otherwise, it will be perceived only by the target and the user.
        /// </summary>
        [DataField]
        public bool SoundPerceivedByOthers = true;

        [DataField]
        public AudioParams SoundParams = new AudioParams()
        {
            Variation = 0.1f
        };
    }

    [Serializable, Flags]
    public enum EffectTargetSpecifier
    {
        /// <summary>
        ///     Popup will be shown above the person executing the verb.
        /// </summary>
        User,
        /// <summary>
        ///     Popup will be shown above the target of the verb.
        /// </summary>
        Target,
        /// <summary>
        ///     The user will see the popup shown above itself, others will see the popup above the target.
        /// </summary>
        UserThenTarget,
        /// <summary>
        ///     The target will see the popup shown above itself, others will see the popup above the user.
        /// </summary>
        TargetThenUser
    }

    [Serializable, Flags]
    public enum ContestType : byte
    {
        Mass = 1,
        Stamina = 1 << 1,
        Health = 1 << 2,
        All = Mass | Stamina | Health,
        None = 0
    }
}

