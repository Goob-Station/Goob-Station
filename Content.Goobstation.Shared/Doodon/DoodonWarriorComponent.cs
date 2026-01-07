using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Doodons;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DoodonWarriorComponent : Component
{
    [DataField, AutoNetworkedField]
    public DoodonOrderType Orders = DoodonOrderType.Follow;

    [DataField, AutoNetworkedField]
    public EntityUid? Papa;

    [DataField, AutoNetworkedField]
    public EntityUid? OrderedTarget;
}
