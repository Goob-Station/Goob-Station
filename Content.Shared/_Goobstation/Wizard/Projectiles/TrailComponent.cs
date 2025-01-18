using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Wizard.Projectiles;

// Make more fields auto networked if you need
[RegisterComponent,NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TrailComponent : Component
{
    /// <summary>
    /// Whether the trail will emit new particles
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool SpawnTrailParticles = true;

    [DataField]
    public float Frequency = 0.2f;

    [DataField]
    public float Lifetime = 1f;

    [DataField]
    public float LerpTime = 0.05f;

    [DataField]
    public float ColorLerpAmount = 0.3f;

    [DataField]
    public float ScaleLerpAmount;

    /// <summary>
    /// If sprite is null, it will draw lines instead
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

    public List<TrailData> TrailData = new();
}

public sealed class TrailData(Vector2 position, Angle angle, Color color, float scale, TimeSpan spawnTime)
{
    public Vector2 Position = position;

    public Angle Angle = angle;

    public Color Color = color;

    public float Scale = scale;

    public TimeSpan SpawnTime = spawnTime;
}
