using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.AnimalAgeing.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangeComponentsOnAgeUpComponent : Component
{
    [DataField]
    public ComponentRegistry AdultComponentsToAdd = new();

    [DataField]
    public ComponentRegistry AdultComponentsToRemove = new();

    [DataField]
    public ComponentRegistry SeniorComponentsToAdd = new();

    [DataField]
    public ComponentRegistry SeniorComponentsToRemove = new();
}
