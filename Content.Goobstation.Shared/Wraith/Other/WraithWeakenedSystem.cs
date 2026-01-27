using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.StatusEffectNew;

namespace Content.Goobstation.Shared.Wraith.Other;

/// <summary>
/// Disables actions for the entity
/// </summary>
public sealed class WraithWeakenedSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WraithWeakenedComponent, StatusEffectAppliedEvent>(OnStatusEffectAdded);
        SubscribeLocalEvent<WraithWeakenedComponent, StatusEffectRemovedEvent>(OnStatusEffectEnded);
    }

    private void OnStatusEffectAdded(Entity<WraithWeakenedComponent> ent, ref StatusEffectAppliedEvent args)
    {
        var ev = new WraithWeakenedAddedEvent();
        RaiseLocalEvent(args.Target, ref ev);

        DisableActions(args.Target);
    }

    private void OnStatusEffectEnded(Entity<WraithWeakenedComponent> ent, ref StatusEffectRemovedEvent args)
    {
        var ev = new WraithWeakenedRemovedEvent();
        RaiseLocalEvent(args.Target, ref ev);

        DisableActions(args.Target, true);
    }

    private void DisableActions(EntityUid uid, bool enabled = false)
    {
        if (!TryComp<ActionsComponent>(uid, out var actions))
            return;

        foreach (var action in actions.Actions)
            _actions.SetEnabled(action, enabled);
    }
}
