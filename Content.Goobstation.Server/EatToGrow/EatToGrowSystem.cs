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

        if (comp.CurrentScale >= comp.MaxGrowth)
            return;

        comp.CurrentScale += comp.Growth;
        comp.CurrentScale = MathF.Min(comp.CurrentScale, comp.MaxGrowth);

        Dirty(eater, comp); // Sync updated growth to client.

        var physics = _entityManager.System<SharedPhysicsSystem>();




        if (_entityManager.TryGetComponent(eater, out FixturesComponent? manager))
        {
            foreach (var (id, fixture) in manager.Fixtures)
            {
                if (fixture.Shape is PhysShapeCircle circle)
                {
                    physics.SetPositionRadius(
                        eater, id, fixture, circle,
                        circle.Position, circle.Radius + comp.Growth/4, manager);
                }
                else
                {
                    throw new NotImplementedException("Only circle fixtures supported for scaling.");
                }
            }
        }

    }

    private void OnGetState(EntityUid uid, EatToGrowComponent comp, ref ComponentGetState args)
    {
        args.State = new EatToGrowComponentState(comp.Growth, comp.MaxGrowth, comp.CurrentScale);
    }
};
