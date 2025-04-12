using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Footprints;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FootprintComponent : Component
{
    [AutoNetworkedField, ViewVariables]
    public List<Footprint> Footprints = [];
}

[Serializable, NetSerializable]
public readonly record struct Footprint(Vector2 Offset, Angle Rotation, Color Color, string State);
