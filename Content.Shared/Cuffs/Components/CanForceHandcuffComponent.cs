using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Cuffs.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedCuffableSystem))]
public sealed partial class CanForceHandcuffComponent : Component
{
    [DataField]
    public EntProtoId HandcuffsId = "Handcuffs";

    [ViewVariables, AutoNetworkedField]
    public EntityUid? Handcuffs;

    [ViewVariables, AutoNetworkedField]
    public BaseContainer? Container;

    [DataField]
    public bool RequireHands = true;

    [DataField]
    public bool Complex = false;
}
