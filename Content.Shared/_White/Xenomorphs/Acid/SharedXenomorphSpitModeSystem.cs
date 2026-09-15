using Content.Shared._White.Weapons.Ranged.Components;
using Content.Shared.Actions;
using Content.Shared.Popups;

namespace Content.Shared._White.Xenomorphs.Acid;

public sealed class SharedXenomorphSpitModeSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphSpitModeComponent, XenomorphSpitModeToggleEvent>(OnToggle);
        SubscribeLocalEvent<XenomorphSpitModeComponent, ComponentStartup>(OnSpitModeStartup);
        SubscribeLocalEvent<PlasmaAmmoProviderComponent, ComponentStartup>(OnAmmoStartup);
    }

    private void OnSpitModeStartup(EntityUid uid, XenomorphSpitModeComponent component, ComponentStartup args)
    {
        ApplyProto(uid, component);
    }

    private void OnAmmoStartup(EntityUid uid, PlasmaAmmoProviderComponent component, ComponentStartup args)
    {
        if (TryComp<XenomorphSpitModeComponent>(uid, out var mode))
            ApplyProto(uid, mode);
    }

    private void OnToggle(EntityUid uid, XenomorphSpitModeComponent component, XenomorphSpitModeToggleEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<PlasmaAmmoProviderComponent>(uid, out _))
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-spit-mode-no-spit"), uid, uid, type: PopupType.SmallCaution);
            return;
        }

        component.ChemicalMode = !component.ChemicalMode;
        Dirty(uid, component);
        ApplyProto(uid, component);

        component.ToggleAction = args.Action.Owner;
        _actions.SetToggled(args.Action.Owner, component.ChemicalMode);

        _popup.PopupEntity(
            Loc.GetString(component.ChemicalMode
                ? "xenomorphs-spit-mode-chemical"
                : "xenomorphs-spit-mode-neurotoxin"),
            uid,
            uid);

        args.Handled = true;
    }

    private void ApplyProto(EntityUid uid, XenomorphSpitModeComponent component)
    {
        if (!TryComp<PlasmaAmmoProviderComponent>(uid, out var ammo))
            return;

        ammo.Proto = component.ChemicalMode ? component.ChemicalProto : component.NeurotoxinProto;
    }
}
