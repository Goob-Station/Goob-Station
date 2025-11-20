using Content.Goobstation.Shared.EatToGrow;
using Content.Server.Nutrition.Components;
using Content.Shared.Mobs; // mobstate changed event
using Content.Shared.Nutrition; // before fully eaten event
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Systems;
using System.Numerics; // using for vectors
namespace Content.Goobstation.Server.EatToGrow;


public sealed class EatToGrowSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FoodComponent, BeforeFullyEatenEvent>(OnFoodEaten);
        SubscribeLocalEvent<EatToGrowComponent, MobStateChangedEvent>(ShrinkOnDeath);
    }

    private void OnFoodEaten(Entity<FoodComponent> ent, ref BeforeFullyEatenEvent args)
    {
        // The entity that ate the food (the mothroach, human, etc.)
        var eater = args.User;

        // Only grow entities that have EatToGrowComponent.
        if (!TryComp<EatToGrowComponent>(eater, out var comp))
            return;
        // if growing would go over the limit, return
        if (comp.CurrentScale >= comp.MaxGrowth)
            return;

        Grow(eater, comp, 1);
    }

    private void Grow(EntityUid eater, EatToGrowComponent comp, float scale)
    {
        // Uses scale variable to  multiply the growth, mainly used for shrinking
        // Add growth
        comp.CurrentScale += comp.Growth;
        comp.CurrentScale = MathF.Min(comp.CurrentScale, comp.MaxGrowth);

        EnsureComp<ScaleVisualsComponent>(eater);

        var appearanceComponent = EnsureComp<AppearanceComponent>(eater);
        if (!_appearance.TryGetData<Vector2>(eater, ScaleVisuals.Scale, out var oldScale, appearanceComponent))
            oldScale = Vector2.One;

        _appearance.SetData(eater, ScaleVisuals.Scale, oldScale + scale * new Vector2(comp.Growth, comp.Growth), appearanceComponent);

        Dirty(eater, comp); // Sync updated growth to client.

        // add 1 to times grown
        comp.TimesGrown += 1;

        // Grow the fixture by 1/4 the growth
        if (TryComp(eater, out FixturesComponent? manager))
        {
            foreach (var (id, fixture) in manager.Fixtures)
            {
                if (fixture.Shape is PhysShapeCircle circle)
                {
                    _physics.SetPositionRadius(
                        eater, id, fixture, circle,
                        circle.Position, circle.Radius + scale * (comp.Growth / 4), manager);
                }
            }
        }
        return; // If fails, return
    }
    private void ShrinkOnDeath(Entity<EatToGrowComponent> eater, ref MobStateChangedEvent args)
    {
        // Copied from TryGrow, just need to grow in reverse
        if (args.NewMobState != MobState.Dead || !TryComp<EatToGrowComponent>(eater, out var comp) || comp.ShrinkOnDeath == false)
        return;

        // shrink the entity
        Grow(eater, comp, -comp.TimesGrown); // uses the negative of times grown to shrink the entity back to normal

        // Reset data on shrink
        comp.CurrentScale = 1f;
        comp.TimesGrown = 0;
    }
};
