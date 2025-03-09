using System.Linq;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._Goobstation.Weapons.UseDelay;

public sealed class UseDelayBlockMeleeSystem : EntitySystem
{
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UseDelayBlockMeleeComponent, AttemptMeleeEvent>(OnMeleeAttempt);
    }

    private void OnMeleeAttempt(Entity<UseDelayBlockMeleeComponent> ent, ref AttemptMeleeEvent args)
    {
        if (!TryComp(ent, out UseDelayComponent? useDelay))
            return;

        if (ent.Comp.Delays.Any(delay => _useDelay.IsDelayed((ent, useDelay), delay)))
            args.Cancelled = true;
    }
}
