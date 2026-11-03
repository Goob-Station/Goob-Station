using Content.Shared.Emp;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModEmpPulseModuleSystem : EntitySystem
{
    [Dependency] private SharedEmpSystem _emp = default!;

    [SubscribeLocalEvent]
    private void OnUsed(Entity<ModEmpPulseModuleComponent> ent, ref ModModuleUsedEvent args)
    {
        _emp.EmpPulse(Transform(args.User).Coordinates, ent.Comp.Range, ent.Comp.EnergyConsumption, ent.Comp.Duration, args.User);
        args.Handled = true;
    }
}
