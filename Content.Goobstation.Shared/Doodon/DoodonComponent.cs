using Robust.Shared.GameObjects;
using Robust.Shared.ViewVariables;

namespace Content.Goobstation.Shared.Doodons;

[RegisterComponent]
public sealed partial class DoodonComponent : Component
{
    [ViewVariables]
    public EntityUid? TownHall;

    [DataField]
    public DoodonHousingType RequiredHousing = DoodonHousingType.None;

    [DataField]
    public bool CountsTowardPopulation = true;
}
