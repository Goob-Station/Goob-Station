using Content.Goobstation.Server.Wizard.Components;
using Content.Server.Emp;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed class EmpImmuneSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<EmpImmuneComponent, EmpAttemptEvent>(OnAttempt);
    }

    private void OnAttempt(Entity<EmpImmuneComponent> ent, ref EmpAttemptEvent args)
    {
        args.Cancel();
    }
}
