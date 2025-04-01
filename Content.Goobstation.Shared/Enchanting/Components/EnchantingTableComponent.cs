using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Marker component added to altars to let items be enchanted on them and allow mob sacraficing to upgrade tiers.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EnchantingTableComponent : Component;
