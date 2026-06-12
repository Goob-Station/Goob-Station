using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class ORTSolarStormComponent : Component
{
    // IMMA CHARGING MY LASER!!!!!!!!!!!!!!
    [DataField]
    public float ChargeTime = 10f;

    /// <summary>
    /// How often the particles spawn in.
    /// </summary>
    [DataField]
    public float ParticleSpawnRate = 0.2f;

    public float CurrentParticleSpawnRate;

    /// <summary>
    /// Increase the spawn rate of particles over time. Technically backwards but whatever.
    /// </summary>
    [DataField]
    public float ParticleIncreaseBy = 0.01f;

    /// <summary>
    /// How far away the particles spawn.
    /// </summary>
    [DataField]
    public float ParticleSpawnRadius = 10f;

    /// <summary>
    /// The prototype of such particles.
    /// </summary>
    [DataField]
    public EntProtoId ParticlePrototype = "ORTSolarParticle";

    /// <summary>
    /// The prototype of the warning, mostly for sprite reasons.
    /// </summary>
    [DataField]
    public EntProtoId WarningPrototype = "ORTSolarStormWarning";
    public EntityUid? WarningEntity;

    /// <summary>
    /// The prototype of the storm, mostly for sprite reasons.
    /// </summary>
    [DataField]
    public EntProtoId StormPrototype = "ORTSolarStorm";
    public EntityUid? StormEntity;

    /// <summary>
    /// The radius of the solar storm's damage area.
    /// </summary>
    [DataField]
    public float StormRadius = 3f;

    /// <summary>
    /// How long the solar storm lasts for.
    /// </summary>
    [DataField]
    public float StormDuration = 8f;

    /// <summary>
    /// Damage dealt by storm every tick.
    /// </summary>
    [DataField]
    public DamageSpecifier StormDamage = new();

    /// <summary>
    /// Sound played as it starts charging.
    /// </summary>
    [DataField]
    public SoundSpecifier ChargeSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/ChargeSolarStorm.ogg");

    /// <summary>
    /// Sound played when...do I even have to say it?
    /// </summary>
    [DataField]
    public SoundSpecifier IMMAFIRINGMYLASERSound = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/stargazer/beam_loop_two.ogg");

    /// <summary>
    /// How long to wait between the warning despawning and the storm spawning.
    /// </summary>
    [DataField]
    public float WaitForIt = 1f;

    #region Glow

    /// <summary>
    /// How brightly the entity should glow. Serves as a cap.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float GlowIntensity = 5;

    /// <summary>
    /// The current glow of the entity, used in tandem with GlowIntensity as a cap.
    /// </summary>
    public float CurrentGlow;

    /// <summary>
    /// How rapidly the glow increaes per tick.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float IncreaseBy = 0.1f;

    public TimeSpan NextUpdate;

    /// <summary>
    /// How quickly to increase the light.
    /// </summary>
    [DataField]
    public TimeSpan Interval = TimeSpan.FromDays(0.1);

    [DataField, AutoNetworkedField]
    public Color LightColor = Color.OrangeRed;

    #endregion

    public float Accumulator;
    public float AccumulatorButCooler; // Particle accumulator
    public float AccumulatorButLame; // Damage accumulator
    public float StormAccumulator; // I ran out of jokes.
    public float AccumulatorBeforeStorm; // For small delay before damage starts ticking, the things we do for love.
    public bool IsCharging;
    public bool IsActive;
    public bool StormSoon;
}
