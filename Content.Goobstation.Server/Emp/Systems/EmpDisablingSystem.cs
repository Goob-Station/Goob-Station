using Content.Server.Emp;

namespace Content.Goobstation.Server.Emp;

public sealed class EmpDisablingSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<EmpDisablingComponent, EmpPulseEvent>(OnEmpPulse);
    }

    private void OnEmpPulse(EntityUid uid, EmpDisablingComponent component, ref EmpPulseEvent args)
    {
        args.Disabled = true;
        args.Duration = component.DisablingTime;
    }
}
