using Content.Shared._Pirate.Plumbing.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Interaction;
using Robust.Shared.Network;

namespace Content.Shared._Pirate.Plumbing.EntitySystems;
//This file currently exists just to block injector popups when you use them on a smart dispenser.
public sealed partial class SharedPlumbingSmartDispenserSystem : EntitySystem
{
    [Dependency] private INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(InteractUsingEvent args)
    {
        if (!_net.IsClient || args.Handled)
            return;

        if (!HasComp<InjectorComponent>(args.Used))
            return;

        if (!HasComp<PlumbingSmartDispenserComponent>(args.Target))
            return;

        args.Handled = true;
    }
}
