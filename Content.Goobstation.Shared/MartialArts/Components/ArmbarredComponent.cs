using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.MartialArts.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ArmbarredComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid Puller;
}
