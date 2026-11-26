using System.Numerics;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.Nullspace;

[RegisterComponent, Access(typeof(NullspaceSystem))]
public sealed partial class NullspaceTrackerComponent : Component
{
    [ViewVariables]
    public NetUserId User;

    [ViewVariables]
    public EntityUid Map;

    [ViewVariables]
    public TimeSpan? LeaveAt;

    public Vector2? LastPosition;

    public float StepDistance;

    public TimeSpan SilenceAt;
}
