using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Content.Shared.Alert;

namespace Content.Shared._White.Xenomorphs.Plasma;

public abstract class SharedPlasmaSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlasmaVesselComponent, ComponentStartup>(OnPlasmaVesselStartup);
        SubscribeLocalEvent<PlasmaVesselComponent, ComponentShutdown>(OnPlasmaVesselShutdown);
        SubscribeLocalEvent<PlasmaVesselComponent, TransferPlasmaActionEvent>(OnPlasmaTransfer);
    }

    private void OnPlasmaVesselStartup(Entity<PlasmaVesselComponent> ent, ref ComponentStartup args)
    {
        _alerts.ShowAlert(ent.Owner, ent.Comp.PlasmaAlert);
    }

    private void OnPlasmaVesselShutdown(Entity<PlasmaVesselComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(ent.Owner, ent.Comp.PlasmaAlert);
    }

    private void OnPlasmaTransfer(Entity<PlasmaVesselComponent> ent, ref TransferPlasmaActionEvent args)
    {
        if (args.Handled || !TryComp<PlasmaVesselComponent>(args.Target, out var plasmaVesselTarget))
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

        _alerts.ShowAlert(uid, component.PlasmaAlert);

        return true;
    }

    /// <summary>
    /// Goobstation - checks if a mob has at least a certain amount of plasma.
    /// </summary>
    public bool HasPlasma(EntityUid uid, FixedPoint2 amount)
        => TryComp<PlasmaVesselComponent>(uid, out var comp)
            && comp.Plasma >= amount;
}
