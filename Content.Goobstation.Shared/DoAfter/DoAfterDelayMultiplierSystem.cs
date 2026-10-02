// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.DoAfter;
using Content.Goobstation.Shared.Cyberware;
using Content.Shared._Shitmed.DoAfter;
using Content.Shared.Body.Systems;

namespace Content.Goobstation.Shared.DoAfter;

public sealed class DoAfterDelayMultiplierSystem : EntitySystem
{
    [Dependency] private readonly CyberneticsSystem _cybernetics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DoAfterDelayMultiplierComponent, GetDoAfterDelayMultiplierEvent>(OnGetMultiplier);
        SubscribeLocalEvent<DoAfterDelayMultiplierComponent, BodyPartRelayedEvent<GetDoAfterDelayMultiplierEvent>>(
            OnGetBodyPartMultiplier);
    }

    private void OnGetBodyPartMultiplier(Entity<DoAfterDelayMultiplierComponent> ent,
        ref BodyPartRelayedEvent<GetDoAfterDelayMultiplierEvent> args)
    {
        if (!_cybernetics.IsEnabled(ent))
            return;

        args.Args.Multiplier *= ent.Comp.Multiplier;
    }

    private void OnGetMultiplier(Entity<DoAfterDelayMultiplierComponent> ent, ref GetDoAfterDelayMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.Multiplier;
    }
}
