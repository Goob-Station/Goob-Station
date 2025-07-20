using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Hierophant.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class HierophantImmuneComponent : Component
{
    [ViewVariables]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan? EndTime;
}
