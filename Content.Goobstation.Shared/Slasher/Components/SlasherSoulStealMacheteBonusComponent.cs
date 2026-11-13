using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Applied to the machete to provide cumulative soul steal bonuses.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class SlasherSoulStealMacheteBonusComponent : Component
{
    [DataField, AutoNetworkedField]
    public DamageSpecifier DamageBonus = new();

    [ViewVariables]
    public DamageSpecifier? BaseThrowDamage;
}
