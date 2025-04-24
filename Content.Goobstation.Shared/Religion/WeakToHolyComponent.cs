using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Religion;

/// <summary>
/// Marks an entity as being an ungodly creature.
/// Yuck.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WeakToHolyComponent : Component
{
    /// <summary>
    /// Is the entity currently standing on a rune?
    /// </summary>
    [ViewVariables]
    public bool IsColliding;

    /// <summary>
    /// Duration between each heal tick while standing on a rule.
    /// </summary>
    [DataField]
    public TimeSpan HealTickDelay = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan NextHealTick;

    // How much holy damage the entity is healed by each tick.
    [DataField]
    public DamageSpecifier HealAmount = new() {DamageDict = new Dictionary<string, FixedPoint2>() {{ "Holy", -4 }}};
}
