using System.Numerics;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Jiggle;

[RegisterComponent, Access(typeof(JiggleSystem))]
public sealed partial class JiggleVisualsComponent : Component
{
    [ViewVariables]
    public Vector2 Offset;

    [ViewVariables]
    public Vector2 Speed;

    [ViewVariables]
    public Vector2 LastVelocity;

    [ViewVariables]
    public Vector2 LastSpriteOffset;

    [ViewVariables]
    public Angle LastSpriteRotation;

    [ViewVariables]
    public List<(TimeSpan Time, Vector2 Impulse)> Pulses = new();

    [ViewVariables]
    public float Sway;

    [ViewVariables]
    public bool Active;

    [ViewVariables]
    public ShaderInstance? Shader;
}
