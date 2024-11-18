using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Tools;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SurgeryToolComponent : Component
{

    [DataField, AutoNetworkedField]
    public SoundSpecifier? StartSound;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? EndSound;
}

/// <summary>
///     Raised on a tool to see if it can be used in a surgery step.
///     If this is cancelled the step can't be completed.
/// </summary>
[ByRefEvent]
public record struct SurgeryToolUsedEvent(EntityUid User, EntityUid Target, bool Cancelled = false);
