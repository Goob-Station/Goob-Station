using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.TileChaser;

/// <summary>
/// Makes a tile chaser depend on anger levels from the spawned owner.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AngerTileChaserComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2 SpeedRange;

    [DataField, AutoNetworkedField]
    public Vector2i StepsRange;

    [DataField]
    public bool Inverse;
}
