using Content.Goobstation.Shared.EatToGrow;
using Content.Shared.Interaction.Events;
using Content.Shared.Nutrition;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using System.Numerics;
namespace Content.Goobstation.Server.EatToGrow;

public sealed class EatToGrowSystem : EntitySystem
{
    public override void Initialize()
    {
        // If you already have a mothroach eating system, hook into its event
        SubscribeLocalEvent<EatToGrowComponent, UseInHandEvent>(OnFoodEaten);
        SubscribeLocalEvent<EatToGrowComponent, ComponentGetState>(OnGetState);
    }

    private void OnFoodEaten(Entity<EatToGrowComponent> ent, ref UseInHandEvent args)
    {

        var (uid, comp) = ent;

        if (comp.CurrentScale >= comp.MaxGrowth)
            return;

        comp.CurrentScale += comp.Growth;
        comp.CurrentScale = MathF.Min(comp.CurrentScale, comp.MaxGrowth);

        Dirty(uid, comp); // syncs to client
    }

    private void OnGetState(EntityUid uid, EatToGrowComponent comp, ref ComponentGetState args)
    {
        args.State = new EatToGrowComponentState(comp.Growth, comp.MaxGrowth, comp.CurrentScale);
    }
};
