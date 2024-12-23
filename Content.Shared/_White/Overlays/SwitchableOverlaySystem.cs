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

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var query = EntityQueryEnumerator<TComp>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.PulseTime <= 0f || comp.PulseAccumulator >= comp.PulseTime || !comp.IsActive)
                continue;

            comp.PulseAccumulator += frameTime;

            if (comp.PulseAccumulator < comp.PulseTime)
            {
                Dirty(uid, comp);
                continue;
            }

            Toggle(uid, comp, false, false);
        }
    }

    private void OnGetState(EntityUid uid, TComp component, ref ComponentGetState args)
    {
        args.State = new SwitchableVisionOverlayComponentState
        {
            IsActive = component.IsActive,
            PulseAccumulator = component.PulseAccumulator,
        };
    }

    private void OnHandleState(EntityUid uid, TComp component, ref ComponentHandleState args)
    {
        if (args.Current is not SwitchableVisionOverlayComponentState state)
            return;

        component.PulseAccumulator = state.PulseAccumulator;

        if (component.PulseTime > 0f && component.PulseAccumulator >= component.PulseTime && !component.IsActive)
            Toggle(uid, component, false, false);
        else
            component.IsActive = state.IsActive;

        RaiseSwitchableOverlayToggledEvent(uid, uid);
        RaiseSwitchableOverlayToggledEvent(uid, Transform(uid).ParentUid);
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
        component.PulseAccumulator = component.PulseTime;
        if (component.ToggleAction != null)
            _actions.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
    }

    private void OnToggle(EntityUid uid, TComp component, TEvent args)
    {
        Toggle(uid, component, component.PulseTime > 0f || !component.IsActive);
        RaiseSwitchableOverlayToggledEvent(uid, args.Performer);
        args.Handled = true;
    }

    private void Toggle(EntityUid uid, TComp component, bool activate, bool playSound = true)
    {
        if (playSound && _net.IsClient && _timing.IsFirstTimePredicted)
        {
            _audio.PlayEntity(activate ? component.ActivateSound : component.DeactivateSound,
                Filter.Local(),
                uid,
                false);
        }

        if (_net.IsServer || component.PulseTime > 0f && !activate) // It is wonky on client otherwise
        {
            component.IsActive = activate;
            component.PulseAccumulator = activate ? 0f : component.PulseTime;
        }

        if (_net.IsServer)
            Dirty(uid, component);
    }

    private void RaiseSwitchableOverlayToggledEvent(EntityUid uid, EntityUid user)
    {
        if (_net.IsServer)
            return;

        var ev = new SwitchableOverlayToggledEvent(user);
        RaiseLocalEvent(uid, ref ev);
    }
}

[ByRefEvent]
public record struct SwitchableOverlayToggledEvent(EntityUid User);
