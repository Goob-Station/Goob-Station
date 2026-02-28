using Robust.Shared.GameObjects;
using Robust.Shared.ViewVariables;

namespace Content.Goobstation.Shared.Doodons;

[RegisterComponent]
public sealed partial class DoodonComponent : Component
{
    // The town hall the doodon is attatched to
    [ViewVariables]
    public EntityUid? TownHall;

    // The required housing a doodon needs
    [DataField]
    public DoodonHousingType RequiredHousing = DoodonHousingType.None;

    // Does this doodon count towards the population
    // Moodons do not, because they are cattle.
    [DataField]
    public bool CountsTowardPopulation = true;
}
