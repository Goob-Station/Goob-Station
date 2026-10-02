using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.ForcedDirectionRotate;

/// <summary>
/// Forces a sprite look direction.
/// If face Target is set the sprite will always face their direction (useful for cinematics and such),
/// and if there isn't one set the user will keep spinning around.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ForcedDirectionRotateComponent : Component
{
    [DataField]
    public List<Direction> Directions = new()
    {
        Direction.South,
        Direction.East,
        Direction.North,
        Direction.West,
    };

    /// <summary>
    /// If set, the sprite is locked to face this entity instead of spinning.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? FaceTarget;

    [DataField]
    public TimeSpan Interval = TimeSpan.FromSeconds(0.1);

    [DataField]
    public int Index;

    [DataField]
    public TimeSpan NextChange;
}
