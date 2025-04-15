// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.Animations;
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
    /// Changing this during runtime may break things.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int ParticleAmount = 1;

    /// <summary>
    /// Limits the total amount of particles that the trail can spawn, if above zero
    /// </summary>
    [DataField]
    public int MaxParticleAmount;

    /// <summary>
    /// If not null, determines spawn position of the particles.
    /// If <see cref="SpawnEntityPosition"/> is not null, it will spawn at coordinates relative to that entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Vector2? SpawnPosition;

    /// <summary>
    /// If not null, particles will spawn at this entity coordinates.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? SpawnEntityPosition;

    /// <summary>
    /// Particles are spawned in a radius around the origin.
    /// </summary>
    [DataField, Animatable]
    public float Radius { get; set; }

    /// <summary>
    /// If this is not null, trail particles will render this entity instead of sprite/lines
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? RenderedEntity;

    /// <summary>
    /// Whether to use <see cref="RenderedEntity"/> rotation (if it is not null), trail entity rotation,
    /// or particle rotation.
    /// </summary>
    [DataField]
    public RenderedEntityRotationStrategy RenderedEntityRotationStrategy;

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
    /// Affects <see cref="AlphaLerpAmount"/>, <see cref="ScaleLerpAmount"/> and <see cref="Velocity"/>
    /// </summary>
    [DataField]
    public float LerpTime = 0.05f;

    /// <summary>
    /// Color alpga lerps to <see cref="AlphaLerpTarget"/> by this amount every <see cref="LerpTime"/> seconds.
    /// </summary>
    [DataField, Animatable]
    public float AlphaLerpAmount { get; set; } = 0.3f;

    /// <summary>
    /// Scale lerps to <see cref="ScaleLerpTarget"/> by this amount every <see cref="LerpTime"/> seconds.
    /// </summary>
    [DataField, Animatable]
    public float ScaleLerpAmount { get; set; }

    /// <summary>
    /// Velocity lerps to <see cref="VelocityLerpTarget"/> by this amount every <see cref="LerpTime"/> seconds.
    /// </summary>
    [DataField, Animatable]
    public float VelocityLerpAmount { get; set; }

    /// <summary>
    /// Particle position lerps to the origin entity position by this amount every <see cref="LerpTime"/> seconds.
    /// </summary>
    [DataField, Animatable]
    public float PositionLerpAmount { get; set; }

    /// <summary>
    /// Color alpha lerps to this value every <see cref="LerpTime"/> seconds.
    /// </summary>
    [DataField, Animatable]
    public float AlphaLerpTarget { get; set; }

    /// <summary>
    /// Scale lerps to this value every <see cref="LerpTime"/> seconds.
    /// </summary>
    [DataField, Animatable]
    public float ScaleLerpTarget { get; set; }

    /// <summary>
    /// Velocity lerps to this value every <see cref="LerpTime"/> seconds.
    /// </summary>
    [DataField, Animatable]
    public float VelocityLerpTarget { get; set; }

    /// <summary>
    /// If sprite is null, it will draw lines instead.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SpriteSpecifier? Sprite;

    [DataField]
    public float Scale = 1f;

    [DataField]
    public string? Shader;

    [DataField]
    public Dictionary<string, IGetShaderData> ShaderData = new();

    [DataField, AutoNetworkedField]
    public Color Color = Color.White;

    [DataField]
    public List<LerpPropertyData> AdditionalLerpData = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public float Accumulator;

    [ViewVariables(VVAccess.ReadOnly)]
    public float LerpAccumulator;

    [ViewVariables(VVAccess.ReadOnly)]
    public int CurIndex;

    [ViewVariables(VVAccess.ReadOnly)]
    public int ParticleCount;

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

[DataDefinition]
public sealed partial class LerpPropertyData
{
    [DataField(required: true)]
    public string Property;

    [DataField(required: true)]
    public float LerpAmount;

    [DataField(required: true)]
    public float Value;

    [DataField(required: true)]
    public float LerpTarget;
}

public enum RenderedEntityRotationStrategy : byte
{
    RenderedEntity = 0,
    Trail,
    Particle
}

[ImplicitDataDefinitionForInheritors]
public partial interface IGetShaderData;

public abstract partial class GetShaderParam : IGetShaderData
{
    [DataField(required: true)]
    public string Param = string.Empty;
}

// Add more data if needed

public sealed partial class GetShaderLocalPositionData : IGetShaderData;

public sealed partial class GetShaderFloatParam : GetShaderParam;