using System.Linq;
using Content.Goobstation.Common.MartialArts;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Server.Grab;

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
        if (args.Direction != null || args.HitEntities.Count is < 0 or > 1)
            return;

        var hitEntity = args.HitEntities.ElementAtOrDefault(0);

        if(hitEntity == default)
            return;

        _pulling.TryStartPull(args.User, hitEntity, grabStageOverride: ent.Comp.GrabStageOverride, escapeAttemptModifier: ent.Comp.EscapeAttemptModifier);
    }
}
