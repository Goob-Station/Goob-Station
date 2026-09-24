using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Graphics.RSI;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Adds heart eyes on top of the users eyes
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

    [DataField]
    public Dictionary<RsiDirection, List<Vector2>> EyeOffsets = new()
    {
        [RsiDirection.South] = new() { new(-0.060f, 0.297f), new(0.031f, 0.297f) },
        [RsiDirection.North] = new(),
        [RsiDirection.East] = new() { new(0.078f, 0.297f) },
        [RsiDirection.West] = new() { new(-0.078f, 0.297f) },
    };
}
