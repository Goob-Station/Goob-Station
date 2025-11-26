using Content.Goobstation.Shared.Wraith.Components;
using Content.Shared._White.RadialSelector;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.Systems;

/// <summary>
/// Handles UI opening of spook menu and activating an action.
/// The actions exist in the server-sided system.
/// </summary>
public sealed class SpookSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpookComponent, ActivatableUIOpenAttemptEvent>(OnUIOpenAttempt);
        SubscribeLocalEvent<SpookComponent, RadialSelectorSelectedMessage>(OnRadialSelectorSelected);
    }
    #region UI
    private void OnUIOpenAttempt(Entity<SpookComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (!HasComp<WraithComponent>(args.User))
            args.Cancel();

        _userInterfaceSystem.SetUiState(ent.Owner,
            RadialSelectorUiKey.Key,
            new TrackedRadialSelectorState(ent.Comp.Actions));
    }

    private void OnRadialSelectorSelected(Entity<SpookComponent> ent, ref RadialSelectorSelectedMessage args)
    {
        DoSelectedAction(ent.Owner, args.SelectedItem);

        _userInterfaceSystem.CloseUi(ent.Owner, RadialSelectorUiKey.Key);
    }
    #endregion
    #region Helpers
    private void DoSelectedAction(EntityUid uid, string? action)
    {
        if (action == null
            || !_prototypeManager.TryIndex(action, out var actionProto)
            || !TryComp<ActionsComponent>(uid, out var actions))
            return;

        foreach (var actionEnt in actions.Actions)
        {
            var metadata = MetaData(actionEnt);
            if (metadata.EntityPrototype != actionProto
                || !TryComp<ActionComponent>(actionEnt, out var actionComp)
                || _actions.IsCooldownActive(actionComp, _timing.CurTime))
                continue;

            _actions.PerformAction(uid, (actionEnt, actionComp));
            break;
        }
    }

    #endregion
}
