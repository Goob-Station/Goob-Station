namespace Content.Goobstation.Shared.Heatlamp.Upgrades;

/// <summary>
///     Component holding data for a heatlamp upgrade. Actual upgrade data is in the other upgrade components.
/// </summary>
/// <remarks>
///     One day, somebody will make a single upgrade system that works for everyone's needs, until then
///     heatlamps/modsuits/lavaland shit will all be their own separate incompatible systems.
/// </remarks>
[RegisterComponent]
public sealed partial class HeatlampUpgradeComponent : Component
{
    /// <summary>
    ///     Maximum number of this type of upgrade that can be installed.
    /// </summary>
    [DataField]
    public int Limit = int.MaxValue;

    /// <summary>
    ///     This upgrade can only be installed on emagged heatlamps.
    /// </summary>
    [DataField]
    public bool EmagOnly = false;
}
