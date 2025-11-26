using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MartialArtsAlert : Component
{
    [DataField] 
    public ProtoId<AlertPrototype> MartialArtProtoId = "MartialArtsStance";
    public override bool SendOnlyToOwner => true;
}