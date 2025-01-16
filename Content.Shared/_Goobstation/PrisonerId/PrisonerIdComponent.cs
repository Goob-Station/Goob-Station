using Content.Shared.Access;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.PrisonerId;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent,NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class PrisonerIdComponent : Component
{

    [DataField,AutoNetworkedField]
    public bool BegunSentence = false;

    [DataField,AutoNetworkedField ]
    public FixedPoint2 SentenceTime = 0;

    /// <summary>
    /// List of access groups that grant access to this reader. Only a single matching group is required to gain access.
    /// A group matches if it is a subset of the set being checked against.
    /// </summary>
    [DataField("access")] [ViewVariables(VVAccess.ReadWrite)]
    public List<HashSet<ProtoId<AccessLevelPrototype>>> AccessLists = new();
}
