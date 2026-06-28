using Content.Shared.Singularity.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Wires;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Content.Omu.Shared.DiodeDisc;
using Content.Shared._DV.Construction;
using YamlDotNet.Core;
using Robust.Shared.Toolshed.Commands.Values;
using Content.Shared.Singularity.Components;

namespace Content.Omu.Shared.DiodeDiscSystem;

public sealed class DiodeDiscSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedWiresSystem _wires = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiodeDiscComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<DiodeDiscComponent, DiodeDiscDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(Entity<DiodeDiscComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not {} target)
            return;

        args.Handled = true;

        var user = args.User;
        if (!HasComp<EmitterComponent>(target))
            return;
        if (HasComp<UpgradedMachineComponent>(target))
            return;
        Dirty(ent);
        var ev = new DiodeDiscDoAfterEvent();
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, ent.Comp.Delay, ev, ent, target, ent));
    }

    private void OnDoAfter(Entity<DiodeDiscComponent> ent, ref DiodeDiscDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (args.Handled || args.Args.Target is not {} target)
            return;

        args.Handled = true;

        var user = args.Args.User;
        // do the upgrading now
        EntityManager.AddComponents(target, ent.Comp.ComponentsToAdd);
        if (_net.IsServer)
            QueueDel(ent);

        if (!TryComp<EmitterComponent>(target, out var blaster))
            return;

        blaster.BoltType = ent.Comp.NewBolt;
    }
}
