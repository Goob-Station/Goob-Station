using Content.Shared.Body.Part;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Body.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class OrganInsertOnUseComponent : Component
{
    [DataField(required: true)]
    public string SlotId;

    [DataField(required: true)]
    public BodyPartType PartType;

    [DataField]
    public bool PreventRemoval = true;
}
