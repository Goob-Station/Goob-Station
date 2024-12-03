using Content.Shared.Actions;
using Content.Shared.Movement.Systems;
using Content.Shared.Damage.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared._EinsteinEngines.Flight.Events;

namespace Content.Shared._EinsteinEngines.Flight;
public abstract class SharedFlightSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
    [Dependency] private readonly StaminaSystem _staminaSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlightComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FlightComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FlightComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
    }

    #region Core Functions
    private void OnStartup(EntityUid uid, FlightComponent component, ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
    }

    private void OnShutdown(EntityUid uid, FlightComponent component, ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(uid, component.ToggleActionEntity);
        if (!TerminatingOrDeleted(uid))
            ToggleActive(uid, false, component);
    }

    public void ToggleActive(EntityUid uid, bool active, FlightComponent component)
    {
        component.On = active;
        component.TimeUntilFlap = 0f;
        _actionsSystem.SetToggled(component.ToggleActionEntity, component.On);
        RaiseNetworkEvent(new FlightEvent(GetNetEntity(uid), component.On, component.IsAnimated));
        _staminaSystem.ToggleStaminaDrain(uid, component.StaminaDrainRate, active, false);
        _movementSpeed.RefreshMovementSpeedModifiers(uid);
        UpdateHands(uid, active);
        Dirty(uid, component);
    }

    private void UpdateHands(EntityUid uid, bool flying)
    {
        if (!TryComp<HandsComponent>(uid, out var handsComponent))
            return;

        if (flying)
            BlockHands(uid, handsComponent);
        else
            FreeHands(uid);
    }

    private void BlockHands(EntityUid uid, HandsComponent handsComponent)
    {
        var freeHands = 0;
        foreach (var hand in _hands.EnumerateHands(uid, handsComponent))
        {
            if (hand.HeldEntity == null)
            {
                freeHands++;
                continue;
            }

            // Is this entity removable? (they might have handcuffs on)
            if (HasComp<UnremoveableComponent>(hand.HeldEntity) && hand.HeldEntity != uid)
                continue;

            _hands.DoDrop(uid, hand, true, handsComponent);
            freeHands++;
            if (freeHands == 2)
                break;
        }
        if (_virtualItem.TrySpawnVirtualItemInHand(uid, uid, out var virtItem1))
            EnsureComp<UnremoveableComponent>(virtItem1.Value);

        if (_virtualItem.TrySpawnVirtualItemInHand(uid, uid, out var virtItem2))
            EnsureComp<UnremoveableComponent>(virtItem2.Value);
    }

    private void FreeHands(EntityUid uid)
    {
        _virtualItem.DeleteInHandsMatching(uid, uid);
    }

    private void OnRefreshMoveSpeed(EntityUid uid, FlightComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (!component.On)
            return;

        args.ModifySpeed(component.SpeedModifier, component.SpeedModifier);
    }

    #endregion
}
public sealed partial class ToggleFlightEvent : InstantActionEvent { }
