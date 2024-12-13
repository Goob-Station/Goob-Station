using Content.Shared.RadialSelector;
using Robust.Shared.GameStates;

namespace Content.Shared.ShortConstruction;

[RegisterComponent, NetworkedComponent]
public sealed partial class ShortConstructionComponent : Component
{
    [DataField(required: true)]
    public List<RadialSelectorEntry> Entries = new();
}
