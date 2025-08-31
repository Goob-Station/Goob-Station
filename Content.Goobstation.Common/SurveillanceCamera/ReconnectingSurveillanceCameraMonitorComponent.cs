using Robust.Shared.Timing;

namespace Content.Goobstation.Common.SurveillanceCamera;

// Dummy component for surveillance monitors waiting for subnets to be refreshed before attempting to reconnect.
[RegisterComponent]
public sealed partial class ReconnectingSurveillanceCameraMonitorComponent : Component
{
    [ViewVariables]
    public GameTick ReconnectToSubnetsDelay = GameTick.Zero;
}
