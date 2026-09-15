using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Robust.Shared.Network;
using Content.Shared._DV.Construction;
using Content.Shared.Singularity.Components;

namespace Content.Shared._Omu.DiodeDisc;

public sealed class DiodeDiscSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiodeDiscComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<DiodeDiscComponent, DiodeDiscDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<DiodeDiscComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { } target)
            return;

        args.Handled = true;

        var user = args.User;
        if (!HasComp<EmitterComponent>(target))
            return;
        if (HasComp<UpgradedMachineComponent>(target))
            return;
        var ev = new DiodeDiscDoAfterEvent();
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.Delay, ev, ent, target, ent));
        Dirty(ent);
    }

    private void OnDoAfter(Entity<DiodeDiscComponent> ent, ref DiodeDiscDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (args.Handled || args.Args.Target is not { } target)
            return;

        args.Handled = true;

        var user = args.Args.User;
        // do the upgrading now
        EntityManager.AddComponents(target, ent.Comp.ComponentsToAdd);
        PredictedQueueDel(ent);

        if (!TryComp<EmitterComponent>(target, out var blaster))
            return;

        blaster.BoltType = ent.Comp.NewBolt;
    }
}
