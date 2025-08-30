// Goobstation start
using Robust.Shared.Timing;

namespace Content.Server.SurveillanceCamera;

// Dummy component for surveillance monitors waiting for subnets to be refreshed before attempting to reconnect.
[RegisterComponent]
public sealed partial class ReconnectingSurveillanceCameraMonitorComponent : Component
{
    [ViewVariables]
    public GameTick ReconnectToSubnetsDelay = GameTick.Zero;
}
// Goobstation end
