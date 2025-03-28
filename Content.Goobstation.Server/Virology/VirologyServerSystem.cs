using Content.Goobstation.Shared.Virology;
using Content.Server.DeviceNetwork;
using Content.Server.DeviceNetwork.Components;
using Content.Server.DeviceNetwork.Systems;
using Content.Shared.DeviceNetwork;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Virology;

public sealed class VirologyServerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetworkSystem = default!;
    [Dependency] private readonly SingletonDeviceNetServerSystem _singletonServerSystem = default!;

    private const float UpdateRate = 3f;
    private float _updateDiff;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VirologyServerComponent, DeviceNetworkPacketEvent>(OnPacketReceived);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // check update rate
        _updateDiff += frameTime;
        if (_updateDiff < UpdateRate)
            return;
        _updateDiff -= UpdateRate;

        var servers = EntityQueryEnumerator<VirologyServerComponent>();

        while (servers.MoveNext(out var id, out var server))
        {
            if (!_singletonServerSystem.IsActiveServer(id))
                continue;

            BroadcastDiseaseInfo(id, server);
        }
    }

    /// <summary>
    /// Adds a disease to memory
    /// </summary>
    private void OnPacketReceived(EntityUid uid, VirologyServerComponent component, DeviceNetworkPacketEvent args)
    {
        // TODO virology
    }

    /// <summary>
    /// Broadcasts currently known diseases
    /// </summary>
    private void BroadcastDiseaseInfo(EntityUid uid, VirologyServerComponent? serverComponent = null, DeviceNetworkComponent? device = null)
    {
        if (!Resolve(uid, ref serverComponent, ref device))
            return;

        var payload = new NetworkPayload()
        {
            [DeviceNetworkConstants.Command] = DeviceNetworkConstants.CmdUpdatedState,
            [VirologyConsoleConstants.NET_STATUS_COLLECTION] = serverComponent.DiseaseInfo
        };

        _deviceNetworkSystem.QueuePacket(uid, null, payload, device: device);
    }
}
