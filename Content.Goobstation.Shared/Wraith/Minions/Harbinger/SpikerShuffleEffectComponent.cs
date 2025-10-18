using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Wraith.Minions.Harbinger;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpikerShuffleEffectComponent : Component;

[NetSerializable, Serializable]
public enum ShuffleVisuals : byte
{
    Shuffling,
}
