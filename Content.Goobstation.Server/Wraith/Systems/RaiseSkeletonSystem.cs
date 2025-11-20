using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Server.Body.Systems;
using Content.Server.Popups;
using Content.Server.Storage.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Mobs.Systems;

namespace Content.Goobstation.Server.Wraith.Systems;

public sealed class RaiseSkeletonSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedRottingSystem _rotting = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RaiseSkeletonComponent, RaiseSkeletonEvent>(OnRaiseSkeleton);
    }

    private void OnRaiseSkeleton(Entity<RaiseSkeletonComponent> ent, ref RaiseSkeletonEvent args)
    {
        // check if we targeted a locker, early return and deploy skeleton if yes
        if (TryComp<EntityStorageComponent>(args.Target, out var entStorage))
        {
            var skeleton = Spawn(ent.Comp.SkeletonProto, Transform(args.Target).Coordinates);

            if (!_entityStorage.Insert(skeleton, args.Target, entStorage))
            {
                Del(skeleton);
                return;
            }

            args.Handled = true;
            return;
        }

        // otherwise, check if target is dead
        if (!_mobState.IsDead(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("wraith-raise-no-corpse"), ent.Owner, ent.Owner);
            return;
        }

        // or rotting
        if (!_rotting.IsRotten(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("wraith-raise-body-refuse"), ent.Owner, ent.Owner);
            return;
        }

        // since both conditions passed, deploy the skeleton and gib them
         Spawn(ent.Comp.SkeletonProto, Transform(args.Target).Coordinates);
        _bodySystem.GibBody(args.Target);

        args.Handled = true;
    }
}
