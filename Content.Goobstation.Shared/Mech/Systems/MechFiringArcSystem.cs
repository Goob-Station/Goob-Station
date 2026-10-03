using Content.Goobstation.Shared.Mech.Components;
using Content.Shared.Mech.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.Mech.Systems;

/// <summary>
/// Only allows the mech to fire within this many degrees of its facing direction.
/// </summary>
public sealed class MechFiringArcSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MechPilotComponent, ShotAttemptedEvent>(OnPilotShotAttempted);
        SubscribeLocalEvent<MechPilotComponent, AttemptMeleeEvent>(OnPilotMeleeAttempt);

        SubscribeLocalEvent<MechFiringArcComponent, ShotAttemptedEvent>(OnMechShotAttempted);
        SubscribeLocalEvent<MechFiringArcComponent, AttemptMeleeEvent>(OnMechMeleeAttempt);
    }

    public bool IsWithinArc(Entity<MechFiringArcComponent> mech, EntityCoordinates target)
    {
        var (mechPos, mechRot) = _transform.GetWorldPositionRotation(mech);
        var targetPos = _transform.ToMapCoordinates(target);
        if (Transform(mech).MapID != targetPos.MapId)
            return true;

        var delta = targetPos.Position - mechPos;
        if (delta.LengthSquared() < 0.05f)
            return true;

        return Math.Abs(Angle.ShortestDistance(mechRot, delta.ToWorldAngle()).Degrees) <= mech.Comp.ArcDegrees;
    }

    private void OnPilotShotAttempted(Entity<MechPilotComponent> pilot, ref ShotAttemptedEvent args) =>
        CheckShot(pilot.Comp.Mech, ref args);

    private void OnMechShotAttempted(Entity<MechFiringArcComponent> mech, ref ShotAttemptedEvent args) =>
        CheckShot(mech, ref args);

    private void OnPilotMeleeAttempt(Entity<MechPilotComponent> pilot, ref AttemptMeleeEvent args) =>
        CheckMelee(pilot.Comp.Mech, ref args);

    private void OnMechMeleeAttempt(Entity<MechFiringArcComponent> mech, ref AttemptMeleeEvent args) =>
        CheckMelee(mech, ref args);

    private void CheckShot(EntityUid mech, ref ShotAttemptedEvent args)
    {
        if (args.Cancelled || !TryComp<MechFiringArcComponent>(mech, out var arc))
            return;

        if (!TryComp<MechComponent>(mech, out var mechComp) || !mechComp.EquipmentContainer.Contains(args.Used))
            return;

        if (args.Used.Comp.ShootCoordinates is not { } target || IsWithinArc((mech, arc), target))
            return;

        _popup.PopupClient(Loc.GetString("mech-fire-arc-blocked"), mech, args.User);

        args.Cancel();
    }

    private void CheckMelee(EntityUid mech, ref AttemptMeleeEvent args)
    {
        if (args.Cancelled || args.Coordinates is not { } target || !TryComp<MechFiringArcComponent>(mech, out var arc))
            return;

        if (IsWithinArc((mech, arc), target))
            return;

        args.Message = Loc.GetString("mech-fire-arc-blocked");
        args.Cancelled = true;
    }
}
