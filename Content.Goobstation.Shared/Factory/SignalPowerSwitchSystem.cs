using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;

namespace Content.Goobstation.Shared.Factory;

public sealed class SignalPowerSwitchSystem : EntitySystem
{
    [Dependency] private readonly SharedDeviceLinkSystem _device = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SignalPowerSwitchComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SignalPowerSwitchComponent, SignalReceivedEvent>(OnSignalReceived);
        SubscribeLocalEvent<SignalPowerSwitchComponent, PowerChangedEvent>(OnPowerChanged);
    }

    private void OnMapInit(Entity<SignalPowerSwitchComponent> ent, ref MapInitEvent args)
    {
        var (uid, comp) = ent;
        _device.EnsureSinkPorts(uid, comp.TogglePort, comp.OnPort, comp.OffPort);
        _device.EnsureSourcePorts(uid, comp.PoweredPort);
    }

    private void OnSignalReceived(Entity<SignalPowerSwitchComponent> ent, ref SignalReceivedEvent args)
    {
        var (uid, comp) = ent;
        var toggle = true;
        if (args.Port == comp.OnPort)
            toggle = !_power.IsPowered(uid);
        else if (args.Port == comp.OffPort)
            toggle = _power.IsPowered(uid);
        else if (args.Port != comp.TogglePort)
            return;

        if (toggle)
            _power.TogglePower(uid);
    }

    private void OnPowerChanged(Entity<SignalPowerSwitchComponent> ent, ref PowerChangedEvent args)
    {
        var (uid, comp) = ent;
        _device.SendSignal(uid, comp.PoweredPort, args.Powered);
    }
}
