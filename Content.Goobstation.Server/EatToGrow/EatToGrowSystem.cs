using Content.Goobstation.Shared.EatToGrow;
using Content.Server.Nutrition.Components;
using Content.Shared.Nutrition;
using Robust.Shared.GameStates;
namespace Content.Goobstation.Server.EatToGrow;

public sealed class EatToGrowSystem : EntitySystem
{
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
    }

    private void OnGetState(EntityUid uid, EatToGrowComponent comp, ref ComponentGetState args)
    {
        args.State = new EatToGrowComponentState(comp.Growth, comp.MaxGrowth, comp.CurrentScale);
    }
};
