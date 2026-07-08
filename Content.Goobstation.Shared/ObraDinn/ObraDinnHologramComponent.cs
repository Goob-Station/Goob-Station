using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.ObraDinn;

[RegisterComponent, NetworkedComponent]
public sealed partial class ObraDinnHologramComponent : Component
{
    /// <summary>
    /// real name of the hologram
    /// </summary>
    [DataField]
    public string RealName = "unknown";

    /// <summary>
    /// distance for listening
    /// </summary>
    [DataField]
    public float MinDistance = 5f;

    /// <summary>
    /// sond played at start and ned as well as when name is revealed
    /// </summary>
    [DataField]
    public SoundPathSpecifier? Sound = new SoundPathSpecifier("/Audio/Items/hiss.ogg");

    /// <summary>
    /// effect played at start and ned as well as when name is revealed
    /// </summary>
    [DataField]
    public string SpawnEffect = "PuddleSparkle";
}
