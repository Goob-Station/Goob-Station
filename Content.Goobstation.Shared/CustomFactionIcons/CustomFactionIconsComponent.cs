using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CustomFactionIcons;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CustomFactionIconsComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ProtoId<FactionIconPrototype>> FactionIcons = [];
}
