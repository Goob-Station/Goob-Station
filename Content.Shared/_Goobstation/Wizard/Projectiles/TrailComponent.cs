using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Wizard.Projectiles;

// Make more fields auto networked if you need
[RegisterComponent,NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TrailComponent : Component
{
    [DataField]
    public float Frequency = 0.2f;

    [DataField]
    public float Lifetime = 1f;

    [DataField]
    public float ColorLerpAmount = 0.3f;

    /// <summary>
    /// Does nothing if sprite is not null
    /// </summary>
    [DataField]
    public float ThicknessLerpAmount;

    /// <summary>
    /// If sprite is null, it will draw lines instead
    /// </summary>
    [DataField]
    public SpriteSpecifier? Sprite;

    /// <summary>
    /// If sprite is null, this determines the thickness of the line
    /// </summary>
    [DataField]
    public float LineThickness = 0.1f;

    [DataField]
    public string? Shader;

    [DataField, AutoNetworkedField]
    public Color Color = Color.White;

    [ViewVariables(VVAccess.ReadOnly)]
    public float Accumulator = 0f;

    [ViewVariables(VVAccess.ReadOnly)]
    public int CurIndex;

    public List<TrailData> TrailData = new();
}

public sealed class TrailData(Vector2 position, Angle angle, Color color, float thickness)
{
    public Vector2 Position = position;

    public Angle Angle = angle;

    public Color Color = color;

    public float Thickness = thickness;
}
