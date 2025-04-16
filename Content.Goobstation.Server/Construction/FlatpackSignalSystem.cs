using Content.Server.DeviceLinking.Events;
using Content.Shared.Construction.Components;
using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Construction;

public sealed class FlatpackSignalSystem : EntitySystem
{
    public static readonly ProtoId<SinkPortPrototype> OnPort = "On";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlatpackCreatorComponent, SignalReceivedEvent>(OnSignalReceived);
    }

    private void OnSignalReceived(Entity<FlatpackCreatorComponent> ent, ref SignalReceivedEvent args)
    {
        if (args.Port != OnPort)
            return;

        // supercode has no API so we have to do this
        var ev = new FlatpackCreatorStartPackBuiMessage();
        RaiseLocalEvent(ent, ev);
    }
}
