using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Enchanting.Components;

/// <summary>
/// Scales <c>MeleeThrowOnHitComponent</c> values by the enchant level.
/// </summary>
[RegisterComponent, NetworkedComponent]
[EntityCategory("Enchants")]
public sealed partial class KnockbackEnchantComponent : Component;
