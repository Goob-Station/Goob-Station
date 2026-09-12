namespace Content.Goobstation.Shared.Weapons.Ranged.HitscanMultiBeam;

/// <summary>
/// Splits a hitscan beem into multiple beams. They are always side by side.
/// </summary>
[RegisterComponent]
public sealed partial class HitscanMultiBeamComponent : Component
{
    /// <summary>
    /// How many beams to fire.
    /// </summary>
    [DataField]
    public int Beams = 2;

    /// <summary>
    /// Distance in tiles between each beam.
    /// </summary>
    [DataField]
    public float Spacing = 0.3f;

    [DataField]
    public bool Split;
}
