using Content.Goobstation.Shared.DeviceNetwork;
using Content.Server.DeviceNetwork.Systems;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;

namespace Content.Goobstation.Server.DeviceNetwork;

public sealed class DeviceCustomFrequencySystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly DeviceNetworkSystem _deviceNetwork = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DeviceCustomFrequencyComponent, BeforeActivatableUIOpenEvent>(OnBeforeActivatableUIOpen);

        Subs.BuiEvents<DeviceCustomFrequencyComponent>(DeviceCustomFrequencyUiKey.Key,
            subs =>
        {
            subs.Event<DeviceCustomReceiveFrequencyChangeMessage>(OnReceiveFrequencyChange);
            subs.Event<DeviceCustomTransmitFrequencyChangeMessage>(OnTransmitFrequencyChange);
        });
    }

    private void OnBeforeActivatableUIOpen(Entity<DeviceCustomFrequencyComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        if (!TryComp<DeviceNetworkComponent>(ent.Owner, out var device))
            return;

        var newState = new DeviceCustomFrequencyUserInterfaceState(device.ReceiveFrequency, device.TransmitFrequency);
        _userInterface.SetUiState(ent.Owner, DeviceCustomFrequencyUiKey.Key, newState);
    }

    private void OnReceiveFrequencyChange(Entity<DeviceCustomFrequencyComponent> ent, ref DeviceCustomReceiveFrequencyChangeMessage args)
    {
        if (!ent.Comp.ReceiveChange
            || !TryComp<DeviceNetworkComponent>(ent.Owner, out var device)
            || args.ReceiveFrequency > ent.Comp.MaxReceiveFrequency
            || args.ReceiveFrequency < ent.Comp.MinReceiveFrequency)
            return;

        _deviceNetwork.SetReceiveFrequency(ent.Owner, args.ReceiveFrequency, device);

        var newState = new DeviceCustomFrequencyUserInterfaceState(device.ReceiveFrequency, device.TransmitFrequency);
        _userInterface.SetUiState(ent.Owner, DeviceCustomFrequencyUiKey.Key, newState);
    }

    private void OnTransmitFrequencyChange(Entity<DeviceCustomFrequencyComponent> ent, ref DeviceCustomTransmitFrequencyChangeMessage args)
    {
        if (!ent.Comp.TransmitChange
            || !TryComp<DeviceNetworkComponent>(ent.Owner, out var device)
            || args.TransmitFrequency > ent.Comp.MaxTransmitFrequency
            || args.TransmitFrequency < ent.Comp.MinTransmitFrequency)
            return;

        _deviceNetwork.SetTransmitFrequency(ent.Owner, args.TransmitFrequency, device);

        var newState = new DeviceCustomFrequencyUserInterfaceState(device.ReceiveFrequency, device.TransmitFrequency);
        _userInterface.SetUiState(ent.Owner, DeviceCustomFrequencyUiKey.Key, newState);
    }
}
