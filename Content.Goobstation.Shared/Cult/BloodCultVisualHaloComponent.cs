using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Cult;

/// <summary>
///     Gives cultists their halos.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultVisualHaloComponent : Component
{
    [DataField]
    public ResPath Sprite = new("/Textures/_Goobstation/Cult/halo.rsi");
}

[Serializable, NetSerializable]
public enum BloodCultHaloKey
{
    Key
}
