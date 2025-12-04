using Robust.Shared.Random;
using Content.Goobstation.Server.DeviceLinking.Components;
using Content.Shared.DeviceLinking.Events;
using Content.Server.DeviceLinking.Systems;

namespace Content.Goobstation.Server.DeviceLinking.Systems;

public sealed class RandomGateSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _deviceLink = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RandomGateComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnSignalReceived(Entity<RandomGateComponent> ent, ref SignalReceivedEvent args)
    {
        var comp = ent.Comp;

        if (args.Port != comp.InputPort)
            return;

        var output = _random.Prob(comp.SuccessProbability);

        if (output != comp.LastOutput)
        {
            comp.LastOutput = output;
            _deviceLink.SendSignal(ent.Owner, comp.OutputPort, output);
        }
    }
}
