using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Wizard.Projectiles;

// Make more fields auto networked if you need to.
// Changing Lifetime and Frequency may lead to unexpected results, especially if frequency is greater than lifetime
[RegisterComponent,NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TrailComponent : Component
{
    /// <summary>
    /// How many particles to spawn each cycle. If it is less than one, no particles will spawn.
    /// Values above one wouldn't work with line trails currently.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int ParticleAmount = 1;

    /// <summary>
    /// Particles are spawned in a radius around the origin.
    /// </summary>
    [DataField]
    public float Radius;

    /// <summary>
    /// If this is not null, trail particles will render this entity instead of sprite/lines
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? RenderedEntity;

    /// <summary>
    /// Whether the trail should slowly fade out even when the entity was deleted.
    /// </summary>
    [DataField]
    public bool SpawnRemainingTrail = true;

    /// <summary>
    /// Used for spread, if <see cref="ParticleAmount"/> is greater than one.
    /// Zero angle faces towards projectile direction.
    /// </summary>
    [DataField]
    public Angle StartAngle;

    /// <summary>
    /// <inheritdoc cref="StartAngle"/>
    /// </summary>
    [DataField]
    public Angle EndAngle;

    /// <summary>
    /// The less this value is, the more frequent the particles will be. This is basically time of each cycle.
    /// </summary>
    [DataField]
    public float Frequency = 0.2f;

    /// <summary>
    /// Lifetime of one particle.
    /// </summary>
    [DataField]
    public float Lifetime = 1f;

    /// <summary>
    /// Velocity of a particle, aimed towards somewhere between <see cref="StartAngle"/> and <see cref="EndAngle"/>.
    /// </summary>
    [DataField]
    public float Velocity;

    /// <summary>
    /// Less value for smoother lerps and more lag. You can get away with much less value, really.
    /// Affects <see cref="ColorLerpAmount"/>, <see cref="ScaleLerpAmount"/> and <see cref="Velocity"/>
    /// </summary>
    [DataField]
    public float LerpTime = 0.05f;

    /// <summary>
    /// Color lerps to <see cref="ColorLerpTarget"/> by this amount every <see cref="LerpTime"/> seconds.
    /// </summary>
    [DataField]
    public float ColorLerpAmount = 0.3f;

    /// <summary>
    /// Scale lerps to <see cref="ScaleLerpTarget"/> by this amount every <see cref="LerpTime"/> seconds.
    /// </summary>
    [DataField]
    public float ScaleLerpAmount;

    /// <summary>
    /// Velocity lerps to <see cref="VelocityLerpTarget"/> by this amount every <see cref="LerpTime"/> seconds.
    /// </summary>
    [DataField]
    public float VelocityLerpAmount;

    /// <summary>
    /// Particle position lerps to the origin entity position by this amount every <see cref="LerpTime"/> seconds.
    /// </summary>
    [DataField]
    public float PositionLerpAmount;

    /// <summary>
    /// Color lerps to this value every <see cref="LerpTime"/> seconds.
    /// </summary>
    [DataField]
    public Color ColorLerpTarget = Color.FromHex("#FFFFFF00");

    /// <summary>
    /// Scale lerps to this value every <see cref="LerpTime"/> seconds.
    /// </summary>
    [DataField]
    public float ScaleLerpTarget;

    /// <summary>
    /// Velocity lerps to this value every <see cref="LerpTime"/> seconds.
    /// </summary>
    [DataField]
    public float VelocityLerpTarget;

    /// <summary>
    /// If sprite is null, it will draw lines instead.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SpriteSpecifier? Sprite;

    [DataField]
    public float Scale = 1f;

    [DataField]
    public string? Shader;

    [DataField, AutoNetworkedField]
    public Color Color = Color.White;

    [ViewVariables(VVAccess.ReadOnly)]
    public float Accumulator;

    [ViewVariables(VVAccess.ReadOnly)]
    public float LerpAccumulator;

    [ViewVariables(VVAccess.ReadOnly)]
    public int CurIndex;

    [ViewVariables(VVAccess.ReadOnly)]
    public MapCoordinates LastCoords = MapCoordinates.Nullspace;

    public List<TrailData> TrailData = new();
}

public sealed class TrailData(
    Vector2 position,
    float velocity,
    MapId mapId,
    Vector2 direction,
    Angle angle,
    Color color,
    float scale,
    TimeSpan spawnTime)
{
    public Vector2 Position = position;

    public float Velocity = velocity;

    public MapId MapId = mapId;

    public Vector2 Direction = direction;

    public Angle Angle = angle;

    public Color Color = color;

    public float Scale = scale;

    public TimeSpan SpawnTime = spawnTime;
}
