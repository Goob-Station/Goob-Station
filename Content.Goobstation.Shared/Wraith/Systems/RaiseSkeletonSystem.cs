using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Atmos.Rotting;
using Content.Shared.Body.Systems;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Wraith.Systems;

public sealed partial class RaiseSkeletonSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedRottingSystem _rotting = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RaiseSkeletonComponent, RaiseSkeletonEvent>(OnRaiseSkeleton);
    }

    private void OnRaiseSkeleton(Entity<RaiseSkeletonComponent> ent, ref RaiseSkeletonEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;
        var target = args.Target;
        var xform = Transform(target);

        if (args.Handled)
            return;

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-fail-target-not-humanoid"), uid, uid);
            return;
        }

        if (!_mobState.IsDead(target))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-fail-target-alive"), uid, uid);
            return;
        }
        //TO DO: Add logic for spawning a skeleton inside a locker. I could not figure out the component and it crashed.

        if (!_rotting.IsRotten(target))
        {
            _popup.PopupPredicted(Loc.GetString("wraith-fail-target-not-rotting"), uid, uid);
            return;
        }
        if (_net.IsServer)
        {
            var voidUid = Spawn(comp.SkeletonProto, xform.Coordinates);
        }
        _bodySystem.GibBody(target);

        args.Handled = true;
    }
}
