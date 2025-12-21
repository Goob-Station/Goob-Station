using Content.Shared._White.Xenomorphs;
using Content.Shared._White.Weapons.Ranged.Components;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Goobstation.Shared.Xenomorph;

public sealed class NeurotoxinGlandSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NeurotoxinGlandComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<NeurotoxinGlandComponent, ToggleAcidSpitEvent>(OnToggleAcidSpit);
        SubscribeLocalEvent<NeurotoxinGlandComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnComponentInit(EntityUid uid, NeurotoxinGlandComponent component, ComponentInit args)
    {
        _actions.AddAction(uid, component.ActionId);

        // Setup gun components
        var plasma = EnsureComp<PlasmaAmmoProviderComponent>(uid);
        plasma.FireCost = component.FireCost;
        plasma.Proto = component.Proto;
        Dirty(uid, plasma);

        var gun = EnsureComp<GunComponent>(uid);
        gun.FireRate = component.FireRate;
        gun.UseKey = false;
        gun.SelectedMode = SelectiveFire.FullAuto;
        gun.AvailableModes = SelectiveFire.FullAuto;
        gun.SoundGunshot = component.SoundGunshot;
        gun.SoundEmpty = component.SoundEmpty;
        Dirty(uid, gun);

        _gun.RefreshModifiers((uid, gun));
    }

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
