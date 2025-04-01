using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Scales <c>DamageOnAttackedComponent.Damage</c> by the enchant level.
/// </summary>
[RegisterComponent, NetworkedComponent]
[EntityCategory("Enchants")]
public sealed partial class ThornsEnchantComponent : Component;
