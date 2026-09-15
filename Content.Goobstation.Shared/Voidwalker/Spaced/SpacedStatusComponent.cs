using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Voidwalker.Spaced;

[RegisterComponent]
public sealed partial class SpacedStatusComponent : Component
{
    [DataField]
    public bool IsInSpace;

    [DataField]
    public bool CheckOnInterval = true;

    [DataField]
    public bool Changed = true;

    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer))]
    public TimeSpan NextSpacedCheck;

    [DataField]
    public TimeSpan SpacedCheckInterval = TimeSpan.FromSeconds(0.25);
}

[ByRefEvent]
public record struct CheckSpacedStatusEvent(bool Spaced, bool Changed);
