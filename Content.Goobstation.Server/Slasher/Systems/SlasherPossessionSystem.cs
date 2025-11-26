using Content.Goobstation.Server.Possession;
using Content.Goobstation.Shared.Slasher.Components;
using Content.Goobstation.Shared.Slasher.Events;
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

    /// <summary>
    /// Slasher - Handles the possession of a target.
    /// </summary>
    private void OnPossess(Entity<SlasherPossessionComponent> ent, ref SlasherPossessionEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<MobStateComponent>(args.Target))
            return;

        // Check if the target has a mindshield and return early
        if (ent.Comp.DoesMindshieldBlock && HasComp<MindShieldComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("possession-fail-target-shielded"), ent.Owner, ent.Owner);
            return;
        }

        // Posses Target
        var ok = _possession.TryPossessTarget(args.Target,
            ent.Owner,
            ent.Comp.PossessionDuration,
            pacifyPossessed: false,
            hideActions: false, // Doesn't actually work I guess
            polymorphPossessor: true);

        // Ensure our actions are not hidden when we posses our target
        if (TryComp<PossessedComponent>(args.Target, out var possessed))
            _actions.UnHideActions(args.Target, possessed.HiddenActions); // required

        args.Handled = true;
    }
}
