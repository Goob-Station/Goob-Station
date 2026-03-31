using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

// Ideally speaking this should be on the heart itself... but this also works.
namespace Content.Goobstation.Shared.Sandevistan;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SandevistanUserComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Active;

    [DataField, AutoNetworkedField]
    public TimeSpan LastEnabled = TimeSpan.Zero;

    [DataField]
    public TimeSpan StatusEffectTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan PopupDelay = TimeSpan.FromSeconds(3);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan NextPopupTime = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public float CurrentLoad = 0f; // Only updated when enabled

    [DataField]
    public float LoadPerActiveSecond = 1f;

    [DataField]
    public float LoadPerInactiveSecond = -0.25f;

    [DataField, AutoNetworkedField]
    public Dictionary<SandevistanState, FixedPoint2> Thresholds = new()
    {
        { SandevistanState.Warning, 15 },
        { SandevistanState.Shaking, 30 },
        { SandevistanState.Damage, 45 },
        { SandevistanState.Disable, 60 },
    };

    /// <summary>
    /// How long the toggle action is disabled after an overload (Disable state).
    /// </summary>
    [DataField]
    public TimeSpan DisableCooldown = TimeSpan.FromSeconds(4);

    [DataField]
    public float StaminaDamage = 5f;

    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "Blunt", 6.5 },
        },
    };

    [DataField, AutoNetworkedField]
    public float MovementSpeedModifier = 2f;

    [DataField, AutoNetworkedField]
    public float AttackSpeedModifier = 2f;

    [DataField, AutoNetworkedField]
    public bool DoAfterSpeedEnabled = true;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public int ColorAccumulator = 0;

    [DataField]
    public float AfterimageInterval = 0.08f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan NextAfterimageTime = TimeSpan.Zero;

    [DataField]
    public EntityUid? PlayingStream;

    [DataField]
    public SoundSpecifier? StartSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/sande_start.ogg");

    [DataField]
    public SoundSpecifier? EndSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/sande_end.ogg");

    [DataField]
    public SoundSpecifier? OverloadSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/sande_overload.ogg");

    [DataField]
    public SoundSpecifier? LoopSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/sande_loop.ogg")
    {
        Params = new AudioParams
        {
            Loop = true,
        }
    };

    [DataField]
    public float LoopSoundDelay = 2.5f;

    /// <summary>
    /// Alert prototype shown when the sandevistan is active, displaying current load.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<AlertPrototype> LoadAlert = "SandevistanLoad";

    #region Slowfield

    /// <summary>
    /// If sandevistan has the slowfield enabled
    /// The slowfield, as the name suggests, slows thing that are nearby similarly to how it works in the show / game.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool SlowfieldEnabled = false;

    /// <summary>
    /// how many tiles to affect.
    /// </summary>
    [DataField]
    public float SlowfieldRadius = 7f;

    /// <summary>
    /// Speed multiplier for mobs in the slowfield.
    /// </summary>
    [DataField]
    public float MobSpeedMultiplier = 0.15f;

    /// <summary>
    /// Speed multiplier for thrown items in the slowfield.
    /// </summary>
    [DataField]
    public float ThrownItemSpeedMultiplier = 0.05f;

    /// <summary>
    /// Speed multiplier for projectiles in the slowfield.
    /// </summary>
    [DataField]
    public float ProjectileSpeedMultiplier = 0.03f;

    #endregion
}
