using Content.Goobstation.Shared.MartialArts.Specific.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Standing;
using Content.Shared.Stunnable;

namespace Content.Goobstation.Shared.MartialArts.Specific;

public sealed partial class JudoArmbarSystem : EntitySystem
{
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private SharedStunSystem _stun = default!;

    [SubscribeLocalEvent]
    private void OnArmbarredStood(Entity<ArmbarredComponent> ent, ref StoodEvent args)
    {
        if (!TryComp<PullableComponent>(ent, out var pullable))
            return;

        _pulling.TryStopPull(ent, pullable, ent.Comp.Puller, true);
        RemComp<ArmbarredComponent>(ent);
    }

    [SubscribeLocalEvent]
    private void OnArmbarStopped(Entity<ArmbarredComponent> ent, ref PullStoppedMessage args)
    {
        if (args.PullerUid != ent.Comp.Puller)
            return;

        _stun.SetKnockdownTime(ent.Owner, TimeSpan.Zero);

        RemComp<ArmbarredComponent>(ent);
    }
}
