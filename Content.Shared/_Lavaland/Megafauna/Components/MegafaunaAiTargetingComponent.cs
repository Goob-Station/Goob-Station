using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._Lavaland.Megafauna.Components;

/// <summary>
/// Component that stores data for what Megafauna had targeted.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MegafaunaAiTargetingComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid? TargetEntity;

    [ViewVariables, AutoNetworkedField]
    public EntityCoordinates? TargetCoordinates;
}
