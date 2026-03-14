using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Shared.Werewolf.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class WerewolfActionComponent : Component
{

    [DataField]
    public float HungerCost = 30f;

    [DataField]
    public bool RequireTransfurmed = false;


    [DataField]
    public LocId NotTransfurmedPopup = "werewolf-action-fail-transfurmed";

    [DataField]
    public LocId NoHungerPopup = "werewolf-action-fail-hunger";
}
