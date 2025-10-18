using Content.Shared.Damage;
using Content.Shared.Nutrition.Components;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Werewolf.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PassiveDamageHungerComponent : Component
{
    /// <summary>
    ///  How much damage to heal based on the hunger threshold.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public Dictionary<HungerThreshold, DamageSpecifier?> HungerThresholds = new();

    /// <summary>
    ///  How often to check this component
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan Interval = TimeSpan.FromSeconds(1);

    [DataField, AutoNetworkedField]
    public TimeSpan NextDamage = TimeSpan.Zero;
}
