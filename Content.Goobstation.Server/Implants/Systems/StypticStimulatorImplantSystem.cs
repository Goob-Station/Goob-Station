// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Implants.Components;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Implants;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class StypticStimulatorImplantSystem : EntitySystem
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StypticStimulatorImplantComponent, ImplantImplantedEvent>(OnImplant);
        SubscribeLocalEvent<StypticStimulatorImplantComponent, EntGotRemovedFromContainerMessage>(OnUnimplanted);
    }

    private void OnImplant(Entity<StypticStimulatorImplantComponent> implant, ref ImplantImplantedEvent args)
    {
        if (!args.Implanted.IsValid() || TerminatingOrDeleted(args.Implanted))
            return;

        implant.Comp.User = args.Implanted;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StypticStimulatorImplantComponent>();
        while (query.MoveNext(out var comp))
        {
            if (comp.NextExecutionTime > _gameTiming.CurTime || comp.User is not { } user)
                continue;

            if (TryComp<BloodstreamComponent>(user, out var bloodstreamComponent))
                _bloodstream.TryModifyBleedAmount((user, bloodstreamComponent), comp.BleedingModifier);

            if (TryComp<DamageableComponent>(user, out var damageableComponent))
                _damageable.TryChangeDamage(user, comp.DamageModifier, true, false, damageableComponent);

            comp.NextExecutionTime = _gameTiming.CurTime + comp.ExecutionDelay;
        }
    }

    private void OnUnimplanted(Entity<StypticStimulatorImplantComponent> implant, ref EntGotRemovedFromContainerMessage args)
    {
        implant.Comp.User = null;
    }
}
