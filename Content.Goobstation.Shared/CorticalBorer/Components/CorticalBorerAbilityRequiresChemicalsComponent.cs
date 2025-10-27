using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.CorticalBorer.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CorticalBorerAbilityRequiresChemicalsComponent : Component
{
    [DataField(required: true)]
    public int Chemicals;
}
