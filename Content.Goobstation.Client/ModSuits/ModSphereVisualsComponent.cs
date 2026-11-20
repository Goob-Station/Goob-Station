using System.Numerics;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.ModSuits;

[RegisterComponent, Access(typeof(ModSphereVisualsSystem))]
public sealed partial class ModSphereVisualsComponent : Component
{
    [ViewVariables]
    public Quaternion Orientation = Quaternion.Identity;

    [ViewVariables]
    public float Morph;

    [ViewVariables]
    public Color PrimaryColor;

    [ViewVariables]
    public Color DarkColor;

    [ViewVariables]
    public Color AccentColor;

    [ViewVariables]
    public TimeSpan LastImpact;

    [ViewVariables]
    public float ImpactStrength;

    [ViewVariables]
    public Vector2 ImpactDirection;

    [ViewVariables]
    public ShaderInstance? Shader;

    [ViewVariables]
    public EntityUid? RollStream;
}
