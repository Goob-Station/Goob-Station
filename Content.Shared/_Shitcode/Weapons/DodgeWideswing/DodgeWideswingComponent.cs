using Robust.Shared.GameStates;
using Robust.Shared.Localization;

namespace Content.Shared._Goobstation.Weapons.DodgeWideswing;

/// <summary>
/// Makes this entity have a chance to dodge a wideswing attack, converting the incoming damage into stamina damage.
/// </summary>
[RegisterComponent]
public sealed partial class DodgeWideswingComponent : Component
{
    [DataField]
    public float Chance = 1f;

    /// <summary>
    /// How much stamina damage to apply per damage from source.
    /// </summary>
    [DataField]
    public float StaminaRatio = 1f;

    /// <summary>
    /// Whether to still evade if knocked down.
    /// </summary>
    [DataField]
    public bool WhenKnockedDown = false;

    [DataField]
    public LocId? PopupId = "wideswing-dodge-generic";
}
