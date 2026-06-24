// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Interaction;
using Content.Shared.Stacks;
using Content.Shared.Whitelist;
using Content.Trauma.Shared.AnimalAgeing;
using Content.Trauma.Shared.Ranching.Components;
using Robust.Shared.Random;

namespace Content.Trauma.Shared.Ranching.Systems;

public sealed partial class PolymorphOnItemsGivenSystem : EntitySystem
{
    [Dependency] private SharedStackSystem _stack = default!;
    [Dependency] private SharedAnimalAgeingSystem _ageing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PolymorphOnItemsGivenComponent, InteractUsingEvent>(OnInteract);
    }

    private void OnInteract(Entity<PolymorphOnItemsGivenComponent> ent, ref InteractUsingEvent args)
    {
        if (_stack.GetCount(args.Used) < ent.Comp.Amount || !_whitelist.IsWhitelistPass(ent.Comp.Whitelist, args.Used))
            return;

        var entity = _random.Pick(ent.Comp.ReplacementEntities);
        _ageing.CopyAndReplaceEntity(entity, args.Target);
        _stack.ReduceCount(args.Used, ent.Comp.Amount);
        RemComp<PolymorphOnItemsGivenComponent>(args.Target);
    }
}
