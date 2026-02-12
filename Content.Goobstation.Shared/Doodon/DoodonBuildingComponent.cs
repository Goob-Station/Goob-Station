using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;

namespace Content.Goobstation.Shared.Doodons;

[RegisterComponent]
public sealed partial class DoodonBuildingComponent : Component
{
    // Does this building provide housing for a specific doodon type
    [DataField]
    public DoodonHousingType HousingType = DoodonHousingType.None;

    // How many doodons of a specific type can this building hold
    [DataField]
    public int HousingCapacity = 0;

    // The town hall it's attatched to
    [ViewVariables]
    public EntityUid? TownHall;

    [ViewVariables]
    public bool Active;

    /// <summary>
    /// How much doodon resin it costs to create this building via the doodon build ability.
    /// </summary>
    [DataField("resinCost")]
    public int ResinCost = 0;

    /// <summary>
    /// Optional: time in seconds to build (if you later add DoAfter).
    /// </summary>
    [DataField("buildTime")]
    public float BuildTime = 0f;

    /// <summary>
    /// Icon used in the doodon build radial menu.
    /// </summary>
    [DataField("buildIcon")]
    public SpriteSpecifier? BuildIcon;
}

public enum DoodonHousingType
{
    None = 0,
    Worker,
    Warrior,
    Moodon,
}
