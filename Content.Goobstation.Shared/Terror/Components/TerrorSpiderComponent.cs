using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Terror.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TerrorSpiderComponent : Component
{
    /// <summary>
    /// Keeps track of how many creatures this individual terror spider has wrapped.
    /// </summary>
    [DataField]
    public int WrappedAmount;

    /// <summary>
    /// Regenerates the terror spider by this amount, multiplied by RegenMultiplier.
    /// </summary>
    [DataField]
    public DamageSpecifier TerrorRegen = new();

    /// <summary>
    /// How often the terror spider regenerates.
    /// </summary>
    [DataField]
    public TimeSpan RegenCooldown = TimeSpan.FromSeconds(3);

    public TimeSpan NextRegenTime;

    [DataField, AutoNetworkedField]
    public float RegenAccumulator = 0f;
}
