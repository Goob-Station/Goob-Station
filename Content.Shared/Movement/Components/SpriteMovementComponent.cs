using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Updates a sprite layer based on whether an entity is moving via input or not.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpriteMovementComponent : Component
{
    /// <summary>
    /// Layer and sprite state to use when moving.
    /// </summary>
    [DataField]
    public Dictionary<string, PrototypeLayerData> MovementLayers = new();

    /// <summary>
    /// Layer and sprite state to use when runnin.
    /// </summary>
    [DataField]
    public Dictionary<string, PrototypeLayerData> RunLayers = new();

    /// <summary>
    /// Layer and sprite state to use when walking.
    /// </summary>
    [DataField]
    public Dictionary<string, PrototypeLayerData> WalkLayers = new();

    /// <summary>
    /// Layer and sprite state to use when not moving.
    /// </summary>
    [DataField]
    public Dictionary<string, PrototypeLayerData> NoMovementLayers = new();
}
