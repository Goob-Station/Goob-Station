using Robust.Shared.GameStates;

namespace Content.Shared.Silicons.StationAi;

/// <summary>
/// Handles the static overlay for station AI.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState] // Shitmed Change - Starlight Abductors
public sealed partial class StationAiOverlayComponent : Component
{
    /// <summary>
    ///     Shitmed Change - Starlight Abductors: Whether the station AI overlay should be allowed to cross grids.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AllowCrossGrid;
}
