using Content.Goobstation.Shared.EatToGrow;
using Content.Server.Nutrition.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Nutrition;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Systems;
using System.Numerics;
using System.Runtime.CompilerServices;
using static Robust.Server.Console.Commands.ScaleCommand;
namespace Content.Goobstation.Server.EatToGrow;


public sealed class EatToGrowSystem : EntitySystem
{
    [Robust.Shared.IoC.Dependency] private readonly AppearanceSystem _appearance = default!;
    [Robust.Shared.IoC.Dependency] private readonly IEntityManager _entityManager = default!;
    [Robust.Shared.IoC.Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
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

        TryGrow(eater, ref comp);
    }

    private void TryGrow(EntityUid eater, ref EatToGrowComponent comp)
    {
        // Add growth
        comp.CurrentScale += comp.Growth;
        comp.CurrentScale = MathF.Min(comp.CurrentScale, comp.MaxGrowth);

        // Scale Command
        EnsureComp<ScaleVisualsComponent>(eater);
        var @event = new ScaleEntityEvent();
        RaiseLocalEvent(eater, ref @event);

        var appearanceComponent = EnsureComp<AppearanceComponent>(eater);
        if (!_appearance.TryGetData<Vector2>(eater, ScaleVisuals.Scale, out var oldScale, appearanceComponent))
            oldScale = Vector2.One;

        _appearance.SetData(eater, ScaleVisuals.Scale, oldScale + new Vector2(comp.Growth, comp.Growth), appearanceComponent);
        // Scale command end
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
                        circle.Position, circle.Radius + comp.Growth / 4, manager);
                }
            }
        }
        return; // If fails, return
    }
    private void ShrinkOnDeath(Entity<EatToGrowComponent> eater, ref MobStateChangedEvent args)
    {

        // Copied from TryGrow, just need to grow in reverse
        if (args.NewMobState == MobState.Dead)
        {
            if (!TryComp<EatToGrowComponent>(eater, out var comp))
                return;

            if (comp.ShrinkOnDeath == true)
            {

                var appearanceComponent = EnsureComp<AppearanceComponent>(eater);
                if (!_appearance.TryGetData<Vector2>(eater, ScaleVisuals.Scale, out var oldScale, appearanceComponent))
                    oldScale = Vector2.One;

                _appearance.SetData(eater, ScaleVisuals.Scale, oldScale - (comp.TimesGrown * new Vector2(comp.Growth, comp.Growth)), appearanceComponent);

                Dirty(eater, comp); // Sync updated growth to client.

                // Grow the fixture by 1/4 the growth
                if (TryComp(eater, out FixturesComponent? manager))
                {
                    foreach (var (id, fixture) in manager.Fixtures)
                    {
                        if (fixture.Shape is PhysShapeCircle circle)
                        {
                            // return radius to normal
                            _physics.SetPositionRadius(
                             eater, id, fixture, circle,
                             circle.Position, circle.Radius - comp.TimesGrown * (comp.Growth / 4), manager); // minus the growth times how many times grown should set it back to normal

                            // set current scale to 1
                            comp.CurrentScale = 1f;
                            // reset times grown
                            comp.TimesGrown = 0;
                        }
                    }
                }
                return;  // If fails, return
            }
            return; // if ShrinkOnDeath is false, return
        }
        return; // if the mob is not dead, return
    }
};
