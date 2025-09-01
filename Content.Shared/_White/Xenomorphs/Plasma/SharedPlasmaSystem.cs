using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Content.Shared.Alert;

namespace Content.Shared._White.Xenomorphs.Plasma;

public abstract class SharedPlasmaSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlasmaVesselComponent, ComponentShutdown>(OnPlasmaVesselShutdown);
        SubscribeLocalEvent<PlasmaVesselComponent, TransferPlasmaActionEvent>(OnPlasmaTransfer);
    }

    private void OnPlasmaVesselShutdown(EntityUid uid, PlasmaVesselComponent component, ComponentShutdown args) =>
        _alerts.ClearAlert(uid, component.PlasmaAlert);

    private void OnPlasmaTransfer(EntityUid uid, PlasmaVesselComponent component, TransferPlasmaActionEvent args)
    {
        if (args.Handled
            || !TryComp<PlasmaVesselComponent>(args.Target, out var plasmaVesselTarget)
            || !ChangePlasmaAmount(uid, -args.Amount, component))
            return;

        ChangePlasmaAmount(args.Target, args.Amount, plasmaVesselTarget);

        args.Handled = true;
    }

    public bool ChangePlasmaAmount(EntityUid uid, FixedPoint2 amount, PlasmaVesselComponent? component = null)
    {
        if (!Resolve(uid, ref component) || component.Plasma + amount < 0)
            return false;

        component.Plasma = FixedPoint2.Min(component.Plasma + amount, component.MaxPlasma);
        Dirty(uid, component);

        RaiseLocalEvent(uid, new PlasmaAmountChangeEvent(component.Plasma));

        _alerts.ShowAlert(uid, component.PlasmaAlert);

        return true;
    }
}
