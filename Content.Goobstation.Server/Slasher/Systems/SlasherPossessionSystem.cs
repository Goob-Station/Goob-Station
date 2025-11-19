using Content.Goobstation.Server.Possession;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Server.Actions;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Network;

namespace Content.Goobstation.Server.Slasher.Systems;

public sealed class SlasherPossessionSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly PossessionSystem _possession = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherPossessionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherPossessionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherPossessionComponent, SlasherPossessionEvent>(OnPossess);
    }

    private void OnMapInit(Entity<SlasherPossessionComponent> ent, ref MapInitEvent args)
    {
        if (!_net.IsServer)
            return;
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<SlasherPossessionComponent> ent, ref ComponentShutdown args)
    {
        if (_net.IsServer)
            _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);
    }

    private void OnPossess(Entity<SlasherPossessionComponent> ent, ref SlasherPossessionEvent args)
    {
        if (args.Handled)
            return;

        if (!_net.IsServer)
        {
            args.Handled = true;
            return;
        }

        if (!HasComp<MobStateComponent>(args.Target))
            return;

        if (ent.Comp.DoesMindshieldBlock && HasComp<MindShieldComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("possession-fail-target-shielded"), ent.Owner, ent.Owner);
            return;
        }

        var ok = _possession.TryPossessTarget(args.Target,
            ent.Owner,
            ent.Comp.PossessionDuration,
            pacifyPossessed: false,
            hideActions: false, // Doesn't actually work I guess
            polymorphPossessor: true);

        if (!ok)
            return;

        if (TryComp<PossessedComponent>(args.Target, out var possessed))
            _actions.UnHideActions(args.Target, possessed.HiddenActions); // required
        args.Handled = true;
    }
}
