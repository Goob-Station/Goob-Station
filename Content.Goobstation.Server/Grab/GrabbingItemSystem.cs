using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Common.Grab;
using Content.Goobstation.Common.MartialArts;
using Content.Shared.Body.Systems;
using Content.Shared.Cuffs;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Pulling.Events;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Grab;

public sealed class GrabbingItemSystem : EntitySystem
{

    [Dependency] private readonly PullingSystem _pulling = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GrabbingItemComponent, MeleeHitEvent>(OnMeleeHitEvent);
    }
    private void OnMeleeHitEvent(Entity<GrabbingItemComponent> ent, ref MeleeHitEvent args)
    {
        if (args.Direction != null)
            return;
        if (args.HitEntities.Count is < 0 or > 1)
            return;
        var hitEntity = args.HitEntities.ElementAtOrDefault(0);
        if(hitEntity == default)
            return;
        _pulling.TryStartPull(args.User, hitEntity);
        _pulling.TryGrab(args.User, hitEntity, grabStageOverride: GrabStage.Hard);
    }
}

[Serializable, NetSerializable]
public sealed partial class GrabBreakDoAfterEvent : SimpleDoAfterEvent
{
}
