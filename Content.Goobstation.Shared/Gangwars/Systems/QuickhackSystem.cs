using Content.Goobstation.Shared.Gangwars.Components;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Gangwars.Systems;

public sealed class QuickhackSystem : EntitySystem
{
    [Dependency] private readonly SharedDoorSystem _doorSystem = default!;
    [Dependency] private readonly SharedChargesSystem _chargesSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<QuickhackComponent, AfterInteractEvent>(OnAfterInteract);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);


        var cur = _timing.CurTime;
        var query = EntityQueryEnumerator<QuickhackComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.FiringResetAt == null || cur < comp.FiringResetAt)
                continue;

            if (comp.FiringTarget is { } target && !TerminatingOrDeleted(target)
                && TryComp<DoorComponent>(target, out var door)
                && CanHack((uid, comp), target, door))
                _doorSystem.TryOpen(target, door, predicted: true);

            comp.FiringResetAt = null;
            comp.FiringTarget = null;
            comp.State = QuickhackState.Idle;
            Dirty(uid, comp);
        }
    }

    private void OnAfterInteract(EntityUid uid, QuickhackComponent comp, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target is not { } target)
            return;

        if (_useDelay.IsDelayed(uid)
            || !TryComp<DoorComponent>(target, out var door)
            || !TryComp<LimitedChargesComponent>(uid, out var charges))
            return;

        args.Handled = true;
        _useDelay.TryResetDelay(uid);

        if (!CanHack((uid, comp), target, door, args.User))
            return;

        _chargesSystem.TryUseCharge((uid, charges));
        _audio.PlayPredicted(comp.UseSound, uid, args.User);

        comp.State = QuickhackState.Firing;
        comp.FiringResetAt = _timing.CurTime + comp.ShootAnimation;
        comp.FiringTarget = target;
        Dirty(uid, comp);
    }

    private bool CanHack(Entity<QuickhackComponent> ent, EntityUid target, DoorComponent door, EntityUid? user = null)
    {
        var canHack = !_doorSystem.IsBolted(target)
            && door.State == DoorState.Closed
            && (user is null || TryComp<LimitedChargesComponent>(ent, out var charges) && !_chargesSystem.IsEmpty((ent.Owner, charges)));

        if (!canHack && user is { } recipient)
        {
            _popup.PopupClient(Loc.GetString("quickhack-failed"), recipient, recipient);
            _audio.PlayPredicted(ent.Comp.FailSound, ent, recipient);
        }

        return canHack;
    }
}
