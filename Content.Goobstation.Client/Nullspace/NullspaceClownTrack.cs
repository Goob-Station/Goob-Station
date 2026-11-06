using System.Numerics;
using Content.Goobstation.Shared.Nullspace;

namespace Content.Goobstation.Client.Nullspace;

public sealed class NullspaceClownTrack
{
    public EntityUid? Uid;
    public Vector2 Position;
    public float Phase;
    public NullspaceClownState State;
    public EntityUid? Target;
}
