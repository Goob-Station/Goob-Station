using System.Linq;
using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.StatusEffect;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Whitelist;
using Robust.Shared.Network;
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

    private EntityQuery<TransformComponent> _transformQuery;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _transformQuery = GetEntityQuery<TransformComponent>();

        SubscribeLocalEvent<WraithCommandComponent, WraithCommandEvent>(OnCommand);
    }

    private void OnCommand(Entity<WraithCommandComponent> ent, ref WraithCommandEvent args)
    {
        _stun.TryStun(args.Target, ent.Comp.StunDuration, false);

        if (_netManager.IsClient)
            return;

        var entities = _lookupSystem.GetEntitiesInRange(ent.Owner, ent.Comp.SearchRange).ToList();
        _random.Shuffle(entities);

        var selectedObjects = 0;
        foreach (var entity in entities)
        {
            if (selectedObjects > ent.Comp.MaxObjects)
                return;

            if (_whitelist.IsBlacklistPass(ent.Comp.Blacklist, entity))
                continue;

            if (_transformQuery.TryComp(entity, out var xform)
                && xform.Anchored)
                continue;

            _throwingSystem.TryThrow(entity, Transform(args.Target).Coordinates, ent.Comp.ThrowSpeed, ent.Owner);
            selectedObjects++;
        }

        // args.Handled = true;
    }
}
