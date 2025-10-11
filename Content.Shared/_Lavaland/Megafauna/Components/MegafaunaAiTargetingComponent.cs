using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._Lavaland.Megafauna.Components;

/// <summary>
/// Component that stores data for what Megafauna had targeted.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MegafaunaAiTargetingComponent : Component
{
    /// <remarks>
    /// All of these entities has to exist, since they are automatically
    /// updated whenever one of them gets deleted.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public Dictionary<string, EntityUid> Entities = new();

    /// <remarks>
    /// All of these coordinates has to be valid, since they are automatically
    /// updated whenever one of them gets deleted.
    /// </remarks>
    [DataField]
    public Dictionary<string, EntityCoordinates> Coordinates = new();
}
