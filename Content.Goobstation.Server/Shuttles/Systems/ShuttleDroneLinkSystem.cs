using Content.Goobstation.Common.Shuttles;
using Content.Server.Shuttles;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Shuttles.Systems;

public sealed class ShuttleDroneLinkSystem : EntitySystem
{
    [Dependency] private readonly ShuttleConsoleSystem _shuttleConsole = default!;

    public static readonly ProtoId<SourcePortPrototype> RemoteDroneSourcePort = "ShuttleDroneTransmitter";
    public static readonly ProtoId<SinkPortPrototype>  RemoteDroneSinkPort = "ShuttleDroneReceiver";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DroneConsoleComponent, LinkAttemptEvent>(OnLinkAttempt);
        SubscribeLocalEvent<DroneConsoleComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<DroneConsoleComponent, DroneGetLinkedShuttleEvent>(OnGetLinkedShuttle);
    }

    private void OnLinkAttempt(Entity<DroneConsoleComponent> ent, ref LinkAttemptEvent ev)
    {
        if (ev.SourcePort != RemoteDroneSourcePort || ev.SinkPort != RemoteDroneSinkPort || HasComp<DroneConsoleComponent>(ev.Sink))
        {
            ev.Cancel();
        }
    }

    private void OnNewLink(Entity<DroneConsoleComponent> ent, ref NewLinkEvent ev)
    {
        _shuttleConsole.RefreshShuttleConsoles();
    }

    private void OnGetLinkedShuttle(Entity<DroneConsoleComponent> ent, ref DroneGetLinkedShuttleEvent ev)
    {
        if (!TryComp<DeviceLinkSourceComponent>(ent.Owner, out var deviceLinkSink))
            return;

        foreach (var linkedPort in deviceLinkSink.LinkedPorts)
        {
            foreach (var port in linkedPort.Value)
            {
                if (port.Source != RemoteDroneSourcePort
                    || !HasComp<ShuttleConsoleComponent>(linkedPort.Key))
                    continue;

                ev.Found = linkedPort.Key;
                break;
            }
        }
    }
}
