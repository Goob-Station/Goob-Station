using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class OrganDeathTraumaComponent : Component
{
    [DataField(required: true)]
    public ProtoId<TraumaTypePrototype> Trauma;
}
