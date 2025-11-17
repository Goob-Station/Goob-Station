using Content.Goobstation.Server.Possession;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Server.Actions;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Slasher.Systems;

public sealed class SlasherPossessionSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly PossessionSystem _possession = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherPossessionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherPossessionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherPossessionComponent, SlasherPossessionEvent>(OnPossess);
    }

    private void OnMapInit(Entity<SlasherPossessionComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<SlasherPossessionComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);
    }

    private void OnPossess(Entity<SlasherPossessionComponent> ent, ref SlasherPossessionEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        if (!HasComp<MobStateComponent>(target))
            return;

        if (ent.Comp.DoesMindshieldBlock && HasComp<MindShieldComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("possession-fail-target-shielded"), ent.Owner, ent.Owner);
            return;
        }

        var ok = _possession.TryPossessTarget(target,
            ent.Owner,
            ent.Comp.PossessionDuration,
            pacifyPossessed: false,
            hideActions: false, // Doesn't actually work I guess
            polymorphPossessor: true);

        if (!ok)
            return;

        if (TryComp<PossessedComponent>(target, out var possessed))
        {
            _actions.UnHideActions(target, possessed.HiddenActions); // required
        }
        args.Handled = true;
    }
}
