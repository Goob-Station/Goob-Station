using Content.Goobstation.Shared.EatToGrow;
using Robust.Client.GameObjects;
using Robust.Shared.GameStates;
using System.Numerics;

namespace Content.Goobstation.Client.EatToGrow;

public sealed class GrowthVisualizerSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<EatToGrowComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(Entity<EatToGrowComponent> ent, ref ComponentHandleState args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (args.Current is not EatToGrowComponentState state)
            return;

        sprite.Scale = new Vector2(state.CurrentScale, state.CurrentScale);
    }
}
