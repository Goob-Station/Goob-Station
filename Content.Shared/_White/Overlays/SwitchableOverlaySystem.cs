using Content.Shared._Goobstation.Flashbang;
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
        SubscribeLocalEvent<TComp, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TComp, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<TComp, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<TComp, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<TComp, ComponentHandleState>(OnHandleState);
        SubscribeLocalEvent<TComp, FlashDurationMultiplierEvent>(OnGetFlashMultiplier);
        SubscribeLocalEvent<TComp, InventoryRelayedEvent<FlashDurationMultiplierEvent>>(OnGetInventoryFlashMultiplier);
    }

    private void OnGetFlashMultiplier(Entity<TComp> ent, ref FlashDurationMultiplierEvent args)
    {
        args.Multiplier *= GetFlashMultiplier(ent);
    }

    private void OnGetInventoryFlashMultiplier(Entity<TComp> ent,
        ref InventoryRelayedEvent<FlashDurationMultiplierEvent> args)
    {
        args.Args.Multiplier *= GetFlashMultiplier(ent);
    }

    private float GetFlashMultiplier(TComp comp)
    {
        if (!comp.IsActive && (comp.PulseTime <= 0f || comp.PulseAccumulator >= comp.PulseTime))
            return 1f;

        return comp.FlashDurationMultiplier;
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_net.IsClient)
            ActiveTick(frameTime);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsServer)
            ActiveTick(frameTime);
    }

    private void ActiveTick(float frameTime)
    {
        var query = EntityQueryEnumerator<TComp>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.PulseTime <= 0f || comp.PulseAccumulator >= comp.PulseTime)
                continue;

            comp.PulseAccumulator += frameTime;

            if (comp.PulseAccumulator < comp.PulseTime)
                continue;

            Toggle(uid, comp, false, false);
            RaiseSwitchableOverlayToggledEvent(uid, uid, comp.IsActive);
            RaiseSwitchableOverlayToggledEvent(uid, Transform(uid).ParentUid, comp.IsActive);
        }
    }

    private void OnGetState(EntityUid uid, TComp component, ref ComponentGetState args)
    {
        args.State = new SwitchableVisionOverlayComponentState
        {
            Color = component.Color,
            IsActive = component.IsActive,
            FlashDurationMultiplier = component.FlashDurationMultiplier,
            ActivateSound = component.ActivateSound,
            DeactivateSound = component.DeactivateSound,
            ToggleAction = component.ToggleAction,
            LightRadius = component is ThermalVisionComponent thermal ? thermal.LightRadius : 0f,
        };
    }

    private void OnHandleState(EntityUid uid, TComp component, ref ComponentHandleState args)
    {
        if (args.Current is not SwitchableVisionOverlayComponentState state)
            return;

        component.Color = state.Color;
        component.FlashDurationMultiplier = state.FlashDurationMultiplier;
        component.ActivateSound = state.ActivateSound;
        component.DeactivateSound = state.DeactivateSound;

        if (component.ToggleAction != state.ToggleAction)
        {
            _actions.RemoveAction(uid, component.ToggleActionEntity);
            component.ToggleAction = state.ToggleAction;
            if (component.ToggleAction != null)
                _actions.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
        }

        if (component is ThermalVisionComponent thermal)
            thermal.LightRadius = state.LightRadius;

        if (component.IsActive == state.IsActive)
            return;

        component.IsActive = state.IsActive;

        RaiseSwitchableOverlayToggledEvent(uid, uid, component.IsActive);
        RaiseSwitchableOverlayToggledEvent(uid, Transform(uid).ParentUid, component.IsActive);
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
    }

    private void OnMapInit(EntityUid uid, TComp component, MapInitEvent args)
    {
        if (component.ToggleActionEntity == null && component.ToggleAction != null)
            _actions.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
    }

    private void OnToggle(EntityUid uid, TComp component, TEvent args)
    {
        Toggle(uid, component, !component.IsActive);
        RaiseSwitchableOverlayToggledEvent(uid, args.Performer, component.IsActive);
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

        if (component.PulseTime > 0f)
        {
            component.PulseAccumulator = activate ? 0f : component.PulseTime;
            return;
        }

        component.IsActive = activate;
        Dirty(uid, component);
    }

    private void RaiseSwitchableOverlayToggledEvent(EntityUid uid, EntityUid user, bool activated)
    {
        var ev = new SwitchableOverlayToggledEvent(user, activated);
        RaiseLocalEvent(uid, ref ev);
    }
}

[ByRefEvent]
public record struct SwitchableOverlayToggledEvent(EntityUid User, bool Activated);
