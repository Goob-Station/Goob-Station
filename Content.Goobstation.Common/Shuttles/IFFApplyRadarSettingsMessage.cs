using Robust.Shared.Serialization;

namespace Content.Goobstation.Common.Shuttles;

[Serializable, NetSerializable]
public sealed class IFFApplyRadarSettingsMessage : BoundUserInterfaceMessage
{
    public Color Color;
    public string? Name;
}
