using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Heatlamp.Upgrades;

/// <summary>
///     Adds components to an upgraded heatlamp.
/// </summary>
[RegisterComponent]
public sealed partial class HeatlampComponentUpgradeComponent : Component
{
    [DataField]
    public ComponentRegistry Components = new();
}
