using System.Numerics;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

// Ideally speaking this should be on the heart itself... but this also works.
namespace Content.Goobstation.Shared.Sandevistan;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class SandevistanUserComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Active;

    [DataField]
    public TimeSpan PopupDelay = TimeSpan.FromSeconds(3);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextPopupTime = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public float CurrentLoad = 0f; // Only updated when enabled

    [DataField, AutoNetworkedField]
    public float LoadPerActiveSecond = 1f;

    [DataField, AutoNetworkedField]
    public float LoadPerInactiveSecond = -0.25f;

    [DataField, AutoNetworkedField]
    public float LoadPerActivation = 2f;

    /// <summary>
    /// This is the required amount of load they need to have available from the closest disable threshold
    /// to enable their sandevistan. Activation cost is also added to this.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ActivationHeadroom = 2f;

    [DataField, AutoNetworkedField]
    public Dictionary<SandevistanState, FixedPoint2> Thresholds = new();

    [DataField]
    public Dictionary<SandevistanState, SandevistanEffect[]> Effects = new()
    {
        { SandevistanState.Shaking,  [new SandevistanJitterEffect()] },
        { SandevistanState.Stamina,  [new SandevistanStaminaDamageEffect()] },
        { SandevistanState.Damage,   [new SandevistanDamageEffect()] },
        { SandevistanState.Knockdown,[new SandevistanKnockdownEffect()] },
        { SandevistanState.Disable,  [new SandevistanDisableEffect()] },
        { SandevistanState.DisableNoAnim, [new SandevistanDisableNoAnimEffect()] },
        { SandevistanState.Death,    [new SandevistanDeathEffect()] },
    };

    [DataField, AutoNetworkedField]
    public float MovementSpeedModifier = 2f;

    [DataField, AutoNetworkedField]
    public float AttackSpeedModifier = 2f;

    [DataField, AutoNetworkedField]
    public float DoAfterModifier = 1.5f;

    [DataField]
    public int ColorAccumulator;

    /// <summary>
    /// Colour of the afterimages. Left null they cycle through the rainbow.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color? AfterimageColor;

    /// <summary>
    /// A new afterimage spawns once the user has moved at least this many tiles from the last one.
    /// If it's too far its considered a teleport and no afterimages get spawned.
    /// </summary>
    [DataField]
    public float AfterimageDistance = 0.3f;

    [DataField]
    public int MaxAfterimagesPerUpdate = 30;

    [DataField]
    public TimeSpan TrailRestartGap = TimeSpan.FromSeconds(0.5);

    [DataField]
    public Vector2 LastAfterimagePos;

    [DataField]
    public TimeSpan LastAfterimageTrackTime;

    [DataField]
    public EntityUid? PlayingStream;

    [DataField]
    public EntityUid? ToggleStream;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? StartSound = new SoundPathSpecifier("/Audio/_Goobstation/Sandevistan/sande_start.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier? EndSound = new SoundPathSpecifier("/Audio/_Goobstation/Sandevistan/sande_end.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier? LoopSound = new SoundPathSpecifier("/Audio/_Goobstation/Sandevistan/sande_loop.ogg")
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

    /// <summary>
    /// Components added to the user + anyone that goes into the slowdown field.
    /// </summary>
    [DataField]
    public ComponentRegistry VisionComponents = new();

    /// <summary>
    /// Components applied to the user only.
    /// </summary>
    [DataField]
    public ComponentRegistry ActivationComponents = new();

    // Cursed
    // A successful counter (see the Counter Stance action, granted by the statveka organ) launches the
    // scripted dash-attack on whoever landed the countered hit.
    #region Dash Attack

    [DataField]
    public CollisionGroup DashObstacleMask = CollisionGroup.Impassable;

    [DataField]
    public int MaxPathExpansions = 800;

    [DataField]
    public float DashAttackMaxRange = 15f; // Just assume everything's in tiles

    /// <summary>
    /// How far each zig-zag hop swings out to the side.
    /// </summary>
    [DataField]
    public float DashAttackZigzagWidth = 3f;

    /// <summary>
    /// Fallback swing multipliers when the normal zigzag patterns get blocked by an object.
    /// </summary>
    [DataField]
    public float[] DashAttackHopLateralScales = [1f, -1f, 0.5f, -0.5f, 0.25f, -0.25f, 0f];

    /// <summary>
    /// How far each hop goes towards the target.
    /// </summary>
    [DataField]
    public float DashAttackHopDistance = 2.5f;

    /// <summary>
    /// After getting this close they stop zigzagging and start the circling phase.
    /// </summary>
    [DataField]
    public float DashAttackCircleEntryRange = 2.5f;

    /// <summary>
    /// Safety cap.
    /// </summary>
    [DataField]
    public int DashAttackMaxHops = 24;

    /// <summary>
    /// How long the user stares at the target after the circling phase before starting the next one.
    /// </summary>
    [DataField]
    public TimeSpan DashAttackWindupDuration = TimeSpan.FromSeconds(0.12);

    /// <summary>
    /// How many times the user attacks them.
    /// </summary>
    [DataField]
    public int DashAttackTrampleCount = 6;

    /// <summary>
    /// How many directions are sampled over the half-circle when lining up a trample pass.
    /// </summary>
    [DataField]
    public int DashAttackTrampleLineSamples = 8;

    /// <summary>
    /// How far to go past the user each attack.
    /// </summary>
    [DataField]
    public float DashAttackTrampleDistance = 6f;

    /// <summary>
    /// The pause before each attack.
    /// </summary>
    [DataField]
    public TimeSpan DashAttackTramplePause = TimeSpan.FromSeconds(0.3);

    /// <summary>
    /// How long each attack takes. Any lower than this and you start getting some engine issues.
    /// </summary>
    [DataField]
    public TimeSpan DashAttackTrampleDashDuration = TimeSpan.FromSeconds(0.02);

    /// <summary>
    /// How long the user is stunned after the full attack.
    /// </summary>
    [DataField]
    public TimeSpan DashAttackTrampleHoldDuration = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Camera shake applied each hit.
    /// </summary>
    [DataField]
    public float DashAttackTrampleCameraKick = 0.5f;

    // TODO: Sandevistan, Uncomment once the cinematic system is merged.
    // [DataField]
    // public ProtoId<CinematicPrototype> DashCinematic = "SandevistanRushdown";
    //
    // [DataField]
    // public float DashCinematicCameraPull = 0.7f;

    /// <summary>
    /// Visual effect spawned on the user each time they dodge a projectile mid-dash.
    /// </summary>
    [DataField]
    public EntProtoId? DashDodgeEffect = "EffectParry";

    [DataField]
    public TimeSpan DashAttackDashDuration = TimeSpan.FromSeconds(0.1);

    [DataField]
    public TimeSpan DashAttackLingerDuration = TimeSpan.FromSeconds(0.05);

    /// <summary>
    /// How close the user is to the victim during the circle phase.
    /// </summary>
    [DataField]
    public float DashAttackCircleRadius = 1f;

    /// <summary>
    /// How long the circling phase lasts before the strike.
    /// </summary>
    [DataField]
    public TimeSpan DashAttackCircleDuration = TimeSpan.FromSeconds(1);

    /// <summary>
    /// How many full loops around the target the user makes during the circling phase.
    /// </summary>
    [DataField]
    public float DashAttackCircleRevolutions = 3.5f;

    [DataField, AutoNetworkedField]
    public bool DashActive;

    [DataField, AutoNetworkedField]
    public EntityUid DashTarget;

    [DataField, AutoNetworkedField]
    public SandevistanDashPhase DashPhase;

    [DataField, AutoNetworkedField]
    public int DashZigzagsDone = -1;

    [DataField, AutoNetworkedField]
    public bool DashFinalApproach;

    [DataField, AutoNetworkedField]
    public Vector2 DashWaypoint;

    [DataField, AutoNetworkedField]
    public Vector2 DashLegStart;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan DashLegStartTime;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan DashCircleEndTime;

    [DataField, AutoNetworkedField]
    public double DashCircleAngle;

    [DataField, AutoNetworkedField]
    public int DashStuckHops;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan DashWindupEndTime;

    [DataField, AutoNetworkedField]
    public int DashTramplePassesDone;

    [DataField, AutoNetworkedField]
    public Vector2 DashTrampleDir;

    [DataField, AutoNetworkedField]
    public float DashTrampleHitFrac;

    [DataField, AutoNetworkedField]
    public bool DashTrampleStruck;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan DashTrampleEndTime;

    #endregion

    #region Slowfield

    /// <summary>
    /// If sandevistan has the slowfield enabled
    /// The slowfield, as the name suggests, slows thing that are nearby similarly to how it works in the show / game.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool SlowfieldEnabled;

    /// <summary>
    /// Launches anyone the user runs into while the slowfield is active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool TrampleEnabled;

    /// <summary>
    /// how many tiles to affect.
    /// </summary>
    [DataField]
    public float SlowfieldRadius = 7f;

    /// <summary>
    /// Speed multiplier for mobs in the slowfield.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MobSpeedMultiplier = 0.15f;

    /// <summary>
    /// Whether items thrown by the user are slowed down in the slowfield.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool SlowThrownItems;

    /// <summary>
    /// Speed multiplier for thrown items in the slowfield.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ThrownItemSpeedMultiplier = 0.05f;

    /// <summary>
    /// Speed multiplier for projectiles in the slowfield.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ProjectileSpeedMultiplier = 0.03f;

    [DataField]
    public float HitDamageMultiplier = 1f;

    /// <summary>
    /// If enabled, when hitting someone with an active sandevistan, it will disable it.
    /// </summary>
    [DataField]
    public bool HitDisables;

    [DataField, AutoNetworkedField]
    public float HitKnockbackStrength;

    /// <summary>
    /// Prevents the hit knockback from taking effect until after the sandevistan is disabled.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool HitKnockbackDelayed;

    [DataField]
    public Dictionary<EntityUid, Vector2> PendingKnockback = new();

    /// <summary>
    /// How far a melee-hit target is thrown.
    /// </summary>
    [DataField]
    public float SlowfieldKnockbackDistance = 6f;

    [DataField]
    public float SlowfieldKnockbackSpeed = 15f;

    /// <summary>
    /// How much of the users own velocity is added to the target.
    /// </summary>
    [DataField]
    public float SlowfieldMomentumScale = 0.5f;

    /// <summary>
    /// Minimum speed required by the user to trample someone.
    /// </summary>
    [DataField]
    public float SlowfieldTrampleMinSpeed = 2.5f;

    [DataField]
    public float SlowfieldTrampleRadius = 0.5f;

    [DataField]
    public DamageSpecifier SlowfieldSlamDamage = new() { DamageDict = new() { { "Blunt", 5 } } };

    [DataField]
    public SoundSpecifier SlowfieldSlamSound = new SoundCollectionSpecifier("MetalThud");

    #endregion
}
