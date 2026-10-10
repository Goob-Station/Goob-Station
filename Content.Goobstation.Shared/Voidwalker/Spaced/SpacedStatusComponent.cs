using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Voidwalker.Spaced;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class SpacedStatusComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsInSpace;

    [DataField]
    public bool CheckOnInterval = true;

    [DataField, AutoNetworkedField]
    public bool Changed = true;

    [DataField(customTypeSerializer:typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextSpacedCheck;

    [DataField]
    public TimeSpan SpacedCheckInterval = TimeSpan.FromSeconds(0.25);
}

[ByRefEvent]
public record struct CheckSpacedStatusEvent(bool Spaced, bool Changed);
