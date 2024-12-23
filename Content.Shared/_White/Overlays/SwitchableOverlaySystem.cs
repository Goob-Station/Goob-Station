using Content.Shared.Actions;
using Content.Shared.Inventory;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._White.Overlays;

public abstract class SwitchableOverlaySystem<TComp, TEvent> : EntitySystem
    where TComp : SwitchableVisionOverlayComponent
    where TEvent : InstantActionEvent
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TComp, TEvent>(OnToggle);
        SubscribeLocalEvent<TComp, ComponentInit>(OnInit);
        SubscribeLocalEvent<TComp, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<TComp, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<TComp, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<TComp, ComponentHandleState>(OnHandleState);
    }

    private void OnGetState(EntityUid uid, TComp component, ref ComponentGetState args)
    {
        args.State = new SwitchableVisionOverlayComponentState
        {
            IsActive = component.IsActive,
        };
    }

    private void OnHandleState(EntityUid uid, TComp component, ref ComponentHandleState args)
    {
        if (args.Current is not SwitchableVisionOverlayComponentState state)
            return;

        component.IsActive = state.IsActive;
    }

    private void OnGetItemActions(Entity<TComp> ent, ref GetItemActionsEvent args)
    {
        if (ent.Comp.ToggleAction != null && args.SlotFlags is not SlotFlags.POCKET and not null)
            args.AddAction(ref ent.Comp.ToggleActionEntity, ent.Comp.ToggleAction);
    }

    private void OnShutdown(EntityUid uid, TComp component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.ToggleActionEntity);
    }

    private void OnInit(EntityUid uid, TComp component, ComponentInit args)
    {
        if (component.ToggleAction != null)
            _actions.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
    }

    private void OnToggle(EntityUid uid, TComp component, TEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        component.IsActive = !component.IsActive;

        if (_net.IsClient)
        {
            _audio.PlayEntity(component.IsActive ? component.ActivateSound : component.DeactivateSound, Filter.Local(), uid, false);
            var ev = new SwitchableOverlayToggledEvent(args.Performer);
            RaiseLocalEvent(uid, ref ev);
        }

        Dirty(uid, component);
        args.Handled = true;
    }
}

[ByRefEvent]
public record struct SwitchableOverlayToggledEvent(EntityUid Performer);
