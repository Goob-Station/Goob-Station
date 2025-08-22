using Content.Server.Shuttles;
using Content.Server.Shuttles.Systems;
using Content.Shared.DeviceLinking.Events;

namespace Content.Server._CorvaxGoob.Shuttles.Systems;

public sealed class ShuttleDroneLinkSystem : EntitySystem
{
    [Dependency] private readonly ShuttleConsoleSystem _shuttleConsole = default!;

    public const string RemoteDroneTag = "DroneShuttleLinkable";

    public const string RemoteDroneSourcePort = "ShuttleDroneTransmitter";
    public const string RemoteDroneSinkPort = "ShuttleDroneReceiver";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DroneConsoleComponent, LinkAttemptEvent>(OnLinkAttempt);
        SubscribeLocalEvent<DroneConsoleComponent, NewLinkEvent>(OnNewLink);
    }

    public void OnLinkAttempt(Entity<DroneConsoleComponent> entity, ref LinkAttemptEvent ev)
    {
        if (ev.SourcePort != RemoteDroneSourcePort || ev.SinkPort != RemoteDroneSinkPort || HasComp<DroneConsoleComponent>(ev.Sink))
        {
            ev.Cancel();
            return;
        }
    }

    public void OnNewLink(Entity<DroneConsoleComponent> entity, ref NewLinkEvent ev)
    {
        _shuttleConsole.RefreshShuttleConsoles();
    }
}
