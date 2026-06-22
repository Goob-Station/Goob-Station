// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Content.Shared.IdentityManagement;
using Content.Trauma.Shared.Decapoids;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Decapoids;

/// <summary>
/// Prevents the entity from being injected with syringes altogether.
/// </summary>
public sealed class ExoskeletonSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly EntityQuery<InjectorComponent> _injectorQuery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExoskeletonComponent, TargetBeforeInjectEvent>(OnBeforeInject);
    }

    private void OnBeforeInject(Entity<ExoskeletonComponent> ent, ref TargetBeforeInjectEvent args)
    {
        if (args.Cancelled || IsHypospray(args.UsedInjector)) // Hyposprays use hypoport system instead
            return;

        args.OverrideMessage = Loc.GetString("exoskeleton-inject-fail", ("target", Identity.Entity(args.TargetGettingInjected, EntityManager)));
        args.Cancel();
    }

    private bool IsHypospray(EntityUid uid) // Copypasted from HypoportSystem because uhh umm
    {
        var comp = _injectorQuery.Comp(uid);
        if (!_proto.Resolve(comp.ActiveModeProtoId, out var mode))
            return false; // invalid injector but not my problem

        // instant injection into mobs means hypospray
        return mode.DelayPerVolume == TimeSpan.Zero && mode.MobTime == TimeSpan.Zero;
    }
}
