using Robust.Shared.GameStates;

namespace Content.Shared.Goobstation.Weapons.Multishot;

/// <summary>
/// Component that allows guns to be shooted with another weapon by holding it in second hand
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MultishotComponent : Component
{
    /// <summary>
    /// Increasing spread when shooting with multiple hands
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SpreadMultiplier = 1.5f;

    /// <summary>
    /// Uid of related weapon
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? RelatedWeapon = default!;
}
