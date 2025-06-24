using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.BurnableByThermite;

[Serializable, NetSerializable]
public enum BurnableByThermiteVisuals : byte
{
    CoveredInThermite,
    OnFireStart,
    OnFireFull
}

[Serializable, NetSerializable]
public enum BurnableByThermiteLayers : byte
{
    Puddle
}
