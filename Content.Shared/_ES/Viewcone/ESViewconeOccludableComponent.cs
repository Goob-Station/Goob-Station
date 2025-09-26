using Content.Shared.Wall;
using Robust.Shared.ComponentTrees;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;

namespace Content.Shared._ES.Viewcone;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ESViewconeOccludableComponent : Component, IComponentTreeEntry<ESViewconeOccludableComponent>
{
    // TODO remove
    [DataField]
    public float BaseAlpha = 1.0f;

    [DataField, AutoNetworkedField]
    public bool OccludeIfAnchored = false;

    /// <summary>
    ///     Whether the occluding should be inverted,
    ///     i.e. the sprite will be invisible while within view, and visible outside of view
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Invert = false;

    public EntityUid? TreeUid { get; set; }
    public DynamicTree<ComponentTreeEntry<ESViewconeOccludableComponent>>? Tree { get; set; }
    public bool AddToTree => true;
    public bool TreeUpdateQueued { get; set; }
}
