using Content.Goobstation.Shared.Devil.Actions;
using Content.Goobstation.Shared.Devil.Components;
using Content.Shared.Actions;

namespace Content.Goobstation.Shared.Devil.Systems;

/// <summary>
/// This system just replicates what Mansus grasp does when drawing a rune, except it is independent from the entity.
/// </summary>
public sealed class DevilHeresySystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DevilHeresyComponent, DevilHeresyEvent>(OnHeresyInstant);
    }

    private void OnHeresyInstant(EntityUid uid, DevilHeresyComponent comp, DevilHeresyEvent args)
    {
        if (!TryComp(args.Performer, out TransformComponent? userTransform))
            return;

        var coords = _transform.GetMapCoordinates(args.Performer);

        var animation = EntityManager.SpawnEntity(comp.AnimationPrototype, coords);
        _transform.AttachToGridOrMap(animation);

        comp.AnimationEntity = animation;
        comp.ElapsedTime = 0f;

        args.Handled = true;
    }

    public override void Update(float frameTime)
    {
        foreach (var (comp, transform) in EntityManager.EntityQuery<DevilHeresyComponent, TransformComponent>())
        {
            if (comp.AnimationEntity == null)
                continue;

            comp.ElapsedTime += frameTime;

            if (comp.ElapsedTime >= comp.DrawTime)
            {
                var coords = _transform.GetMapCoordinates(comp.AnimationEntity.Value);
                var rune = EntityManager.SpawnEntity(comp.RunePrototype, coords);
                _transform.AttachToGridOrMap(rune);

                EntityManager.DeleteEntity(comp.AnimationEntity.Value);
                comp.AnimationEntity = null;
            }
        }
    }
}
