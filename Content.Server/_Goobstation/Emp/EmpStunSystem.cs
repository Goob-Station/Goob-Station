using Content.Server.Emp;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Stunnable;

namespace Content.Server._Goobstation.Emp;

public sealed class EmpStunSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SiliconComponent, EmpPulseEvent>(OnEmpParalyze);
        SubscribeLocalEvent<BorgChassisComponent, EmpPulseEvent>(OnEmpParalyze);
    }

    private void OnEmpParalyze(EntityUid uid, Component component, ref EmpPulseEvent args)
    {
        args.Affected = true;
        args.Disabled = true;
        var duration = args.Duration;
        if (duration > TimeSpan.FromSeconds(15))
            duration = TimeSpan.FromSeconds(15);
        _stun.TryParalyze(uid, duration, true);
    }
}
