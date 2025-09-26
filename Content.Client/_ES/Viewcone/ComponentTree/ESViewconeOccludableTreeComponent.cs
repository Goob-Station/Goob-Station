using Content.Shared._ES.Viewcone;
using Robust.Shared.ComponentTrees;
using Robust.Shared.Physics;

namespace Content.Client._ES.Viewcone.ComponentTree;

[RegisterComponent]
public sealed partial class ESViewconeOccludableTreeComponent : Component, IComponentTreeComponent<ESViewconeOccludableComponent>
{
    public DynamicTree<ComponentTreeEntry<ESViewconeOccludableComponent>> Tree { get; set; }
}
