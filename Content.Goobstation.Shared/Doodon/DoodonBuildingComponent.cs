using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.ViewVariables;

namespace Content.Goobstation.Shared.Doodons;

[RegisterComponent]
public sealed partial class DoodonBuildingComponent : Component
{
    [DataField]
    public DoodonHousingType HousingType = DoodonHousingType.None;

    [DataField]
    public int HousingCapacity = 0;

    [ViewVariables]
    public EntityUid? TownHall;

    [ViewVariables]
    public bool Active;
}

public enum DoodonHousingType
{
    None = 0,
    Worker,
    Warrior,
    Moodon,
}
