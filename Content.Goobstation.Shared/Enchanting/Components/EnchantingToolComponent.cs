using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Component added to bibles that lets them interact with <see cref="EnchanterComponent"/>
/// on an altar to enchant an item.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EnchantingToolComponent : Component;
