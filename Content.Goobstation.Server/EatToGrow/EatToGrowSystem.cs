using Content.Goobstation.Shared.EatToGrow;
using Content.Server.Nutrition.Components;
using Content.Shared.Nutrition;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Systems;
using System.Numerics;
using static Robust.Server.Console.Commands.ScaleCommand;
namespace Content.Goobstation.Server.EatToGrow;

public sealed class EatToGrowSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<FoodComponent, AfterFullyEatenEvent>(OnFoodEaten);
        SubscribeLocalEvent<EatToGrowComponent, ComponentGetState>(OnGetState);
    }

    private void OnFoodEaten(Entity<FoodComponent> ent, ref AfterFullyEatenEvent args)
    {
        // The entity that ate the food (the mothroach, human, etc.)
        var eater = args.User;

        // Only grow entities that have EatToGrowComponent.
        if (!TryComp<EatToGrowComponent>(eater, out var comp))
            return;
        // if growing would go over the limit, return
        if (comp.CurrentScale >= comp.MaxGrowth)
            return;
        // Add growth
        comp.CurrentScale += comp.Growth;
        comp.CurrentScale = MathF.Min(comp.CurrentScale, comp.MaxGrowth);

        Dirty(eater, comp); // Sync updated growth to client.

        // Grow the fixture by 1/4 the growth
        if (TryComp(eater, out FixturesComponent? manager))
        {
            foreach (var (id, fixture) in manager.Fixtures)
            {
                if (fixture.Shape is PhysShapeCircle circle)
                {
                    _physics.SetPositionRadius(
                        eater, id, fixture, circle,
                        circle.Position, circle.Radius + comp.Growth/4, manager);
                }
            }
        }
        return; // If fails, return

    }

    private void OnGetState(EntityUid uid, EatToGrowComponent comp, ref ComponentGetState args)
    {
        args.State = new EatToGrowComponentState(comp.Growth, comp.MaxGrowth, comp.CurrentScale);
    }
};
