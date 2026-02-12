using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Doodons;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DoodonWarriorComponent : Component
{
    // The order the warrior follows
    [DataField, AutoNetworkedField]
    public DoodonOrderType Orders = DoodonOrderType.Follow;

    // The specific Papa Doodon the warrior follows
    [DataField, AutoNetworkedField]
    public EntityUid? Papa;

    // The target that the Papa doodon pointed at
    [DataField, AutoNetworkedField]
    public EntityUid? OrderedTarget;
}
