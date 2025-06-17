using Content.Shared._Starlight.CollectiveMind;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Implants.Components;

[RegisterComponent]
public sealed partial class ImplantGrantCollectiveMindComponent : Component
{
    [DataField]
    public ProtoId<CollectiveMindPrototype> CollectiveMind;
}
