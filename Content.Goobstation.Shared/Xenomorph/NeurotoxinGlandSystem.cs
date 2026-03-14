using Content.Shared._White.Xenomorphs;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Goobstation.Shared.Xenomorph;

public sealed class NeurotoxinGlandSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NeurotoxinGlandComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<NeurotoxinGlandComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<NeurotoxinGlandComponent, ToggleAcidSpitEvent>(OnToggleAcidSpit);
        SubscribeLocalEvent<NeurotoxinGlandComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnMapInit(EntityUid uid, NeurotoxinGlandComponent component, MapInitEvent args) =>
        _actions.AddAction(uid, component.ActionId);

    private void OnComponentShutdown(EntityUid uid, NeurotoxinGlandComponent component, ComponentShutdown args) =>
        _actions.RemoveAction(uid, component.Action);

    private void OnShotAttempted(EntityUid uid, NeurotoxinGlandComponent component, ref ShotAttemptedEvent args)
    {
        // Prevent shooting if the gland is not active. It still lets them shove.
        if (!component.Active)
            args.Cancel();
    }

    private void OnToggleAcidSpit(EntityUid uid, NeurotoxinGlandComponent component, ToggleAcidSpitEvent args)
    {
        // Toggle the active state
        component.Active = !component.Active;

        if (component.Active)
            _popup.PopupPredicted(Loc.GetString("neurotoxin-gland-activated"), uid, uid);
        else
            _popup.PopupPredicted(Loc.GetString("neurotoxin-gland-deactivated"), uid, uid);

        Dirty(uid, component);
        args.Handled = true;
    }
}
