using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Doors;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Prying.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Slasher.Systems;

public sealed partial class SlasherDoorJackSystem : EntitySystem
{
    [Dependency] private SharedAirlockSystem _airlock = default!;
    [Dependency] private IGameTiming _timing = default!;

    [SubscribeLocalEvent]
    private void OnBeforeClosed(EntityUid uid, SlasherJackedDoorComponent comp, BeforeDoorClosedEvent args)
    {
        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnBeforeAutoClose(EntityUid uid, SlasherJackedDoorComponent comp, BeforeDoorAutoCloseEvent args)
    {
        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnBeforePry(Entity<SlasherJackedDoorComponent> ent, ref BeforePryEvent args)
    {
        args.Cancelled = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlasherJackedDoorComponent>();
        while (query.MoveNext(out var uid, out var jacked))
        {
            if (_timing.CurTime < jacked.ExpiresAt)
                continue;

            RemCompDeferred<SlasherJackedDoorComponent>(uid);

            if (HasComp<AirlockComponent>(uid))
                _airlock.UpdateAutoClose(uid);
        }
    }
}
