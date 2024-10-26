using Content.Shared.Actions;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._White.Overlays;

public abstract class SwitchableOverlaySystem<TComp, TEvent> : EntitySystem
    where TComp : SwitchableOverlayComponent
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
        SubscribeLocalEvent<TComp, ComponentRemove>(OnRemove);
    }

    private void OnRemove(EntityUid uid, TComp component, ComponentRemove args)
    {
        _actions.RemoveAction(uid, component.ToggleActionEntity);
        UpdateVision(uid, false);
    }

    private void OnInit(EntityUid uid, TComp component, ComponentInit args)
    {
        _actions.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
        UpdateVision(uid, component.IsActive);
    }

    protected virtual void UpdateVision(EntityUid uid, bool active) { }

    private void OnToggle(EntityUid uid, TComp component, TEvent args)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        component.IsActive = !component.IsActive;

        if (_net.IsClient)
            _audio.PlayEntity(component.IsActive ? component.ActivateSound : component.DeactivateSound, Filter.Local(), uid, false);

        args.Handled = true;
        UpdateVision(uid, component.IsActive);
    }
}
