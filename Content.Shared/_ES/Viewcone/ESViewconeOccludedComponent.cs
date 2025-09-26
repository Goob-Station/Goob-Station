using Robust.Shared.GameStates;

namespace Content.Shared._ES.Viewcone;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ESViewconeOccludedComponent : Component
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
}
