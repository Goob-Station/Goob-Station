using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Server.Medical.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Bloodsuckers.Systems;

public sealed class BloodsuckerHealthAnalyzerSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteractUsing(Entity<BloodsuckerComponent> ent, ref InteractUsingEvent args)
    {
        if (!HasComp<HealthAnalyzerComponent>(args.Used))
            return;

        if (ent.Comp.IsMasquerading)
            return;

        _popup.PopupEntity(
            Loc.GetString("bloodsucker-health-analyzer-detected", ("target", ent.Owner)),
            args.User, args.User, PopupType.MediumCaution);
    }
}
