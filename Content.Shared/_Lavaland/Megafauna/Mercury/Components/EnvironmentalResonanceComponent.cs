using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Spawns an entity in two corners of the screen, then skips a tile in a specific direction and does it again.
/// Which corners of the screen are decided by a bool.
/// Each row, in the case of the Megafauna, is actually an invisible projectile moving towards a direction by using DirectionalMovementComponent.
/// That projectile spawns in warning entities that spawn in beams.
/// If you wish to use this system, that is the ideal way to do so. It is generic enough to be done.
/// </summary>

[RegisterComponent]
public sealed partial class EnvironmentalResonanceComponent : Component
{
    /// <summary>
    /// Prototype that moves horizontally towards the right.
    /// </summary>
    [DataField]
    public EntProtoId RightPrototype;

    /// <summary>
    /// Prototype that moves horizontally towards the left.
    /// </summary>
    [DataField]
    public EntProtoId LeftPrototype;

    /// <summary>
    /// Prototype that moves horizontally towards the right.
    /// </summary>
    [DataField]
    public EntProtoId UpPrototype;

    /// <summary>
    /// Prototype that moves horizontally towards the left.
    /// </summary>
    [DataField]
    public EntProtoId DownPrototype;

    /// <summary>
    /// Self-explanatory, determines how far away from the entity the prototype will spawn.
    /// </summary>
    [DataField]
    public float HorizontalOffset;

    /// <summary>
    /// Self-explanatory, determines how far away from the entity the prototype will spawn.
    /// </summary>
    [DataField]
    public float VerticalOffset;

    /// <summary>
    /// How many tiles to skip before spawning the next entity.
    /// </summary>
    [DataField]
    public float TileSkip = 2f;

    /// <summary>
    /// Number of rows to spawn entities in.
    /// </summary>
    [DataField]
    public int RowNumber;

}
