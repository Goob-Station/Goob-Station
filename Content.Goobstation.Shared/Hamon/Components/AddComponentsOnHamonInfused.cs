using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Hamon.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class AddComponentsOnHamonInfusedComponent : Component
{
    [DataField (required: true)]
    public ComponentRegistry Components;
}
