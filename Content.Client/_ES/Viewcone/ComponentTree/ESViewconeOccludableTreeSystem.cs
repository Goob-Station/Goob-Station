using System.Numerics;
using Content.Shared._ES.Viewcone;
using Robust.Shared.ComponentTrees;
using Robust.Shared.Physics;

namespace Content.Client._ES.Viewcone.ComponentTree;

/// <summary>
///     Handles gathering sprites to modify alpha in the viewcone overlays
/// </summary>
public sealed class ESViewconeOccludableTreeSystem : ComponentTreeSystem<ESViewconeOccludableTreeComponent, ESViewconeOccludableComponent>
{
    protected override bool DoFrameUpdate => true;
    protected override bool DoTickUpdate => false;
    protected override bool Recursive => false;

    protected override Box2 ExtractAabb(in ComponentTreeEntry<ESViewconeOccludableComponent> entry, Vector2 pos, Angle rot)
    {
        throw new NotImplementedException();
    }
}
