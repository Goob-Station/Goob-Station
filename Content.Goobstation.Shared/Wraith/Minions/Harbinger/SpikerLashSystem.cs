using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Stunnable;

namespace Content.Goobstation.Shared.Wraith.Minions.Harbinger;

public sealed class SpikerLashSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpikerLashComponent, SpikerLashEvent>(OnSpikerLash);
    }

    private void OnSpikerLash(Entity<SpikerLashComponent> ent, ref SpikerLashEvent args)
    {
        // TODO: popup here
        _stunSystem.TryKnockdown(args.Target, ent.Comp.KnockdownDuration, false);

        if (!TryComp<BloodstreamComponent>(args.Target, out var blood))
            return;

        _bloodstream.TryModifyBloodLevel((args.Target, blood), ent.Comp.BleedAmount);
        _bloodstream.TryModifyBleedAmount((args.Target, blood), blood.MaxBleedAmount);

        args.Handled = true;
    }
}
