using System.Linq;
using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Whitelist;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Wraith.Systems;

/// <summary>
/// This handles the command ability of Wraith.
/// Hurls a few nearby loose objects at the chosen target.
/// </summary>
public sealed class WraithCommandSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;
    [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetManager _netManager = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WraithCommandComponent, WraithCommandEvent>(OnCommand);
    }

    //TO DO: Would be nice if the objects temporarily floated upwards before floating towards the target.
    //Just cosmetic, so leaving for part 2.
    private void OnCommand(Entity<WraithCommandComponent> ent, ref WraithCommandEvent args)
    {
        _stun.TryStun(args.Target, ent.Comp.StunDuration, false);

        if (_netManager.IsClient)
            return;

        var found = new HashSet<Entity<PullableComponent>>();
        _lookupSystem.GetEntitiesInRange(Transform(ent.Owner).Coordinates, ent.Comp.SearchRange, found);
        var foundList = found.ToList();
        _random.Shuffle(foundList);

        foreach (var entity in foundList)
        {
            if (_whitelist.IsBlacklistPass(ent.Comp.Blacklist, entity))
                continue;

            _throwingSystem.TryThrow(entity, Transform(args.Target).Coordinates, ent.Comp.ThrowSpeed, ent.Owner);
        }

        args.Handled = true;
    }
}
