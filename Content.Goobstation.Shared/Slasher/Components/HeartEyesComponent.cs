using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Renders heart eyes on top of an entity's eyes.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HeartEyesComponent : Component
{
    [DataField, AutoNetworkedField]
    public Color Color = Color.FromHex("#FF5C8A");

    [DataField, AutoNetworkedField]
    public float Size = 0.073f;

    [DataField, AutoNetworkedField]
    public float ChromaticAberration = 0.12f;

    [DataField, AutoNetworkedField]
    public float Shake = 0.02f;

    /// <summary>
    /// Eye positions per facing direction.
    /// </summary>
    [DataField]
    public Dictionary<Direction, List<Vector2>> EyeOffsets = new()
    {
        [Direction.South] = new() { new(-0.060f, 0.297f), new(0.031f, 0.297f) },
        [Direction.North] = new(),
        [Direction.East] = new() { new(0.078f, 0.297f) },
        [Direction.West] = new() { new(-0.078f, 0.297f) },
    };
}
