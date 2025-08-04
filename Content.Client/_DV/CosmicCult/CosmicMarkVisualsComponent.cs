using System.Numerics;

namespace Content.Client._DV.CosmicCult;

/// <summary>
/// This is used to apply an offset to the star mark, and select the proper state for the subtle mark depending on the species.
/// </summary>
[RegisterComponent]
public sealed partial class CosmicMarkVisualsComponent : Component
{
    [DataField]
    public Vector2 Offset = Vector2.Zero;

    [DataField]
    public string SubtleState = "default";
}
