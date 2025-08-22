using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Events;

[Serializable, NetSerializable]
public sealed class IFFApplyRadarSettingsMessage : BoundUserInterfaceMessage
{
    public Color Color;
    public string? Name;
}
