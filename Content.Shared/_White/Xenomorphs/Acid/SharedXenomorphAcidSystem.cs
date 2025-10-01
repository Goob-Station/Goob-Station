using Content.Shared._White.Other;
using Content.Shared._White.Xenomorphs.Acid.Components;
using Content.Shared.Coordinates;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._White.Xenomorphs.Acid;

public abstract class SharedXenomorphAcidSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphAcidComponent, AcidActionEvent>(OnXenomorphAcidActionEvent);
    }

    private void OnXenomorphAcidActionEvent(EntityUid uid, XenomorphAcidComponent component, AcidActionEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<StructureComponent>(args.Target)) // TODO: This should check whether the target is a structure.
        {
            _popup.PopupClient(Loc.GetString("xenomorphs-acid-not-corrodible", ("target", args.Target)), uid, uid);
            return;
        }

        if (HasComp<AcidCorrodingComponent>(args.Target))
        {
            _popup.PopupClient(Loc.GetString("xenomorphs-acid-already-corroding", ("target", args.Target)), uid, uid);
            return;
        }

        args.Handled = true;

        if (_net.IsClient)
            return;

        var acid = SpawnAttachedTo(component.AcidId, args.Target.ToCoordinates());
        var acidCorroding = new AcidCorrodingComponent
        {
            Acid = acid,
            AcidExpiresAt = Timing.CurTime + component.AcidLifeTime,
            DamagePerSecond = component.DamagePerSecond
        };
        AddComp(args.Target, acidCorroding);
    }
}
