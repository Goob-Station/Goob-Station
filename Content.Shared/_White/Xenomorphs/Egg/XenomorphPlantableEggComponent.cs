using Robust.Shared.Prototypes;

namespace Content.Shared._White.Xenomorphs.Egg;

/// <summary>
/// Handheld egg produced by an empress. Activate on a free tile to plant a growing <see cref="XenomorphEggComponent"/>.
/// </summary>
[RegisterComponent]
public sealed partial class XenomorphPlantableEggComponent : Component
{
    [DataField]
    public EntProtoId PlantedPrototype = "XenomorphEgg";
}
