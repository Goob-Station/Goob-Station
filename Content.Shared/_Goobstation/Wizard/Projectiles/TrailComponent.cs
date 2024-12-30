using System.Numerics;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Wizard.Projectiles;

[RegisterComponent]
public sealed partial class TrailComponent : Component
{
    [DataField]
    public float Frequency = 0.2f;

    [DataField]
    public float Lifetime = 1f;

    [DataField]
    public float LerpAmount = 0.3f;

    [DataField(required: true)]
    public SpriteSpecifier Sprite;

    [ViewVariables(VVAccess.ReadOnly)]
    public float Accumulator = 0f;

    [ViewVariables(VVAccess.ReadOnly)]
    public int CurIndex;

    public List<TrailData> TrailData = new();
}

public sealed class TrailData(Vector2 position, Angle angle, Color color)
{
    public Vector2 Position = position;

    public Angle Angle = angle;

    public Color Color = color;
}
