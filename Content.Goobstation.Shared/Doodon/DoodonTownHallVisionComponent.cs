using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Doodons;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class DoodonTownHallVisionComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool ShowTownHallRadius;

    [DataField] public EntProtoId ToggleRadiusAction = "ActionToggleTownHallRadius";
    [DataField] public EntityUid? ToggleRadiusActionEntity;
}
