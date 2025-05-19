using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.Silicons.StationAi;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StationAiCoreSpriteComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public Dictionary<StationAiState, Dictionary<StationAiVisualLayers, SpriteSpecifier>> Visuals = new();

    [ViewVariables, AutoNetworkedField]
    public Dictionary<StationAiState, Dictionary<StationAiVisualLayers, SpriteSpecifier>> OldVisuals = new();

    [ViewVariables, AutoNetworkedField]
    public EntityUid? CoreUid;
}
