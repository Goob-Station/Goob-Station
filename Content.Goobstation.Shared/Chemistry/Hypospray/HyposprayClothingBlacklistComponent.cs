using Content.Shared.Inventory;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Chemistry.Hypospray;

/// <summary>
/// Prevents entities wearing blacklisted clothing from being injected.
/// </summary>
[RegisterComponent, AutoGenerateComponentState]
public sealed partial class HyposprayClothingBlacklistComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityWhitelist Blacklist;

    /// <summary>
    /// Slots to check.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SlotFlags Slots;
}
