using System.Numerics;

namespace Content.Goobstation.Client.Projectiles;

/// <summary>
/// Speed line effect for auto-dodging entities.
/// </summary>
[RegisterComponent]
public sealed partial class SpeedLinesEffectComponent : Component
{
    /// <summary>
    /// How long the effect lasts.
    /// </summary>
    [DataField]
    public float Duration = 0.35f;

    /// <summary>
    /// Diameter of the effect in world units.
    /// </summary>
    [DataField]
    public float Size = 1.75f;

    /// <summary>
    /// Color of the strokes.
    /// </summary>
    [DataField]
    public Color Color = Color.FromHex("#0A0D12");

    [DataField]
    public Vector2 Direction = Vector2.UnitX;

    [DataField]
    public float Seed;

    [DataField]
    public float Progress;
}
