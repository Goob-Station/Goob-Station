using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(fieldDeltas:true)]
public sealed partial class MartialArtsAlertComponent : Component
{
    [DataField] 
    public ProtoId<AlertPrototype> MartialArtProtoId = "MartialArtsStance";
    public override bool SendOnlyToOwner => true;
    [DataField, AutoNetworkedField]
    public bool Stance; // True = Defensive, False = Passive
    
}

public sealed partial class ToggleMartialArtsStanceEvent : BaseAlertEvent;