using Robust.Shared.GameStates;

namespace Content.Shared.Mood;

/// <summary>
/// Networked mood values used by shared prediction and contest math.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NetMoodComponent : Component
{
    [DataField, AutoNetworkedField]
    public float CurrentMoodLevel;

    [DataField, AutoNetworkedField]
    public float NeutralMoodThreshold;
}
