using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

[RegisterComponent]
public sealed partial class EnvironmentalResonanceComponent : Component
{
    /// <summary>
    /// Prototype that moves horizontally towards the right.
    /// </summary>
    [DataField]
    public EntProtoId RightPrototype = "MoveRight";

    /// <summary>
    /// Prototype that moves horizontally towards the left.
    /// </summary>
    [DataField]
    public EntProtoId LeftPrototype = "MoveLeft";

    /// <summary>
    /// Prototype that moves horizontally towards the right.
    /// </summary>
    [DataField]
    public EntProtoId UpPrototype = "MoveUp";

    /// <summary>
    /// Prototype that moves horizontally towards the left.
    /// </summary>
    [DataField]
    public EntProtoId DownPrototype = "MoveDown";

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
