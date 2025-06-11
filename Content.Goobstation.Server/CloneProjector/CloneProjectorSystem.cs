using Content.Goobstation.Shared.CloneProjector;
using Content.Goobstation.Shared.CloneProjector.Clone;
using Content.Server.Emp;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.CloneProjector;

public sealed partial class CloneProjectorSystem : SharedCloneProjectorSystem
{
    [Dependency] private readonly StunSystem _stunSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CloneComponent, EmpPulseEvent>(OnEmpPulse);

        InitializeProjector();
        InitializeClone();
    }

    private void OnEmpPulse(Entity<CloneComponent> clone, ref EmpPulseEvent args)
    {
        if (clone.Comp.HostProjector is not { } projector
            || clone.Comp.HostEntity is not { } host)
            return;

        args.Disabled = true;
        args.Affected = true;

        var duration = args.Duration;
        if (duration > TimeSpan.FromSeconds(15))
            duration = TimeSpan.FromSeconds(15);

        TryInsertClone(projector, true);
        _stunSystem.TryParalyze(host, duration, true);

        var destroyedPopup = Loc.GetString("gemini-projector-clone-destroyed");
        _popup.PopupEntity(destroyedPopup, host, host, PopupType.LargeCaution);
    }
}
