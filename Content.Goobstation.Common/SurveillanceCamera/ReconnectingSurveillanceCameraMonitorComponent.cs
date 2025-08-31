namespace Content.Goobstation.Common.SurveillanceCamera;

// Dummy component for surveillance monitors waiting for subnets to be refreshed before attempting to reconnect.
[RegisterComponent]
public sealed partial class ReconnectingSurveillanceCameraMonitorComponent : Component
{
    /*
        Tick 0 - same tick the request to refresh subnetworks reaches the surveillance camera monitor system
        Queue a disconnect packet for all known subnetworks
        Clean the known subnets dictionary
        Queue a subnet discovery packet to all possible subnets (depends on prototype, sec monitor has everything but entertainment and tv has only entertainment)
        Tick 1 - the packets queue from previous tick is now being processed this tick triggering events in the camera routers
        Camera routers queue packets with their information to be sent back to the monitor system
        Tick 2 - the packets queue from previous tick is now being processed this tick triggering events in the camera monitor
        Each packet triggers an event in monitor system and a subnetwork address is added to known subnets dictionary
        Tick 3 - the delay is finished and now the method to connect to the known subnets is called from the update loop of the monitor system
    */

    [ViewVariables]
    public uint TicksDelay = 3;
}
