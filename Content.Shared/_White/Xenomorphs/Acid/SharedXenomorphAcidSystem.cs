using Content.Shared.FixedPoint;
using Content.Shared._White.Actions;
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
    [Dependency] private readonly PlasmaCostActionSystem _plasmaCost = default!; // Goobstation

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphAcidComponent, AcidActionEvent>(OnXenomorphAcidActionEvent);
    }

    private void OnXenomorphAcidActionEvent(EntityUid uid, XenomorphAcidComponent component, AcidActionEvent args)
    {
        if (args.Handled)
            return;

        // Check if this is a plasma-cost action and get the cost
        // Goobstart
        TryComp<PlasmaCostActionComponent>(args.Action, out var plasmaCost);
        var plasmaCostValue = plasmaCost?.PlasmaCost ?? FixedPoint2.Zero;

        // Check plasma cost before proceeding
        if (plasmaCostValue > FixedPoint2.Zero && !_plasmaCost.HasEnoughPlasma(uid, plasmaCostValue))
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-acid-not-enough-plasma"), uid, uid, type: PopupType.SmallCaution);
            return;
        }

        if (!HasComp<StructureComponent>(args.Target)) // TODO: This should check whether the target is a structure.
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-acid-not-corrodible", ("target", args.Target)), uid, uid, type: PopupType.SmallCaution);
            return;
        }

        if (HasComp<AcidCorrodingComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("xenomorphs-acid-already-corroding", ("target", args.Target)), uid, uid, type: PopupType.SmallCaution);
            return;
        }

        // Deduct the plasma cost after all checks pass
        if (plasmaCostValue > FixedPoint2.Zero)
            _plasmaCost.DeductPlasma(uid, plasmaCostValue);

        args.Handled = true;
        _popup.PopupEntity(Loc.GetString("xenomorphs-acid-apply", ("target", args.Target)), uid, uid, type: PopupType.Small);
        // Goobstation end

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
