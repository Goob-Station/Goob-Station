using Content.Goobstation.Shared.Voidwalker.Components;
using Content.Goobstation.Shared.Voidwalker.TemporarilyDisableCollision;
using Content.Shared.Tag;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Voidwalker.GlassPasser;

public sealed partial class GlassPasserSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GlassPasserComponent, PreventCollideEvent>(OnPreventCollide);
    }

    /// <inheritdoc />
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GlassPasserComponent>();

        while (query.MoveNext(out _, out var comp))
            CleanupPassedEntities(comp, _timing.CurTime);
    }

    private void OnPreventCollide(Entity<GlassPasserComponent> entity, ref PreventCollideEvent args)
    {
        if (!_tag.HasAnyTag(args.OtherEntity, entity.Comp.PassableTags)
            && !_tag.HasTag(args.OtherEntity, entity.Comp.VoidedStructureTag))
            return;

        args.Cancelled = true;

        if (_tag.HasTag(args.OtherEntity, entity.Comp.VoidedStructureTag))
            return;

        EnsureComp<TemporarilyDisableCollisionComponent>(args.OtherEntity);

        entity.Comp.EntitiesPassed[args.OtherEntity] = _timing.CurTime + entity.Comp.EntityPassedAddedComponentsDuration;
        EnsureComp<VoidedVisualsComponent>(args.OtherEntity);
    }

    private void CleanupPassedEntities(GlassPasserComponent comp, TimeSpan curTime)
    {
        var toRemove = new List<EntityUid>();

        foreach (var (ent, expiry) in comp.EntitiesPassed)
            if (curTime > expiry)
                toRemove.Add(ent);

        foreach (var ent in toRemove)
        {
            comp.EntitiesPassed.Remove(ent);
            RemComp<VoidedVisualsComponent>(ent);
        }
    }
}
