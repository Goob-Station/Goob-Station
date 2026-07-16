using Content.Shared.Polymorph;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components.Mobs;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class DiseasedRatComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ProtoId<DiseasedRatFormUnlockPrototype>> DiseasedRatForms = new();
}

[Prototype]
public sealed partial class DiseasedRatFormUnlockPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public int FilthRequired;

    [DataField(serverOnly: true)]
    public EntProtoId? Entity;

    [DataField(serverOnly: true)]
    public HashSet<ComponentTransferData>? TransferComponents = new();
}
