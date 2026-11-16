using Content.Goobstation.Shared.ModSuits.Components;
using Content.Shared.Emp;

namespace Content.Goobstation.Shared.ModSuits.Modules;

public sealed partial class ModEmpShieldModuleSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private void OnSuitEmpAttempt(Entity<ModSuitComponent> ent, ref EmpAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        foreach (var module in ent.Comp.ModulesContainer.ContainedEntities)
        {
            if (!TryComp<ModEmpShieldModuleComponent>(module, out var shield)
                || !TryComp<ModModuleComponent>(module, out var moduleComp)
                || shield.RequireActive && !moduleComp.Active)
                continue;

            args.Cancelled = true;
            return;
        }
    }
}
