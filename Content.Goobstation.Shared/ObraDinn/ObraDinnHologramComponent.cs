using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.ObraDinn;

[RegisterComponent, NetworkedComponent]
public sealed partial class ObraDinnHologramComponent : Component
{
    [DataField]
    public string RealName = "unknown";
    [DataField]
    public float MinDistance = 5f;
    [DataField]
    public SoundPathSpecifier? Sound = new SoundPathSpecifier("/Audio/Items/hiss.ogg");
    [DataField]
    public string SpawnEffect = "PuddleSparkle";
}
