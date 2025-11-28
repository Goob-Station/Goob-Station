using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChameleonSkinComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionChameleonSkin";

    [DataField]
    public EntityUid? ActionEnt;

    /// <summary>
    /// Popup when toggled on.
    /// </summary>
    [DataField]
    public LocId ActivePopup = "changeling-chameleon-start";

    /// <summary>
    /// Popup when toggled off.
    /// </summary>
    [DataField]
    public LocId InactivePopup = "changeling-chameleon-end";

    /// <summary>
    /// Popup when set on fire while invisible.
    /// </summary>
    [DataField]
    public LocId IgnitedPopup = "changeling-chameleon-fire";

    /// <summary>
    /// Popup when attempting to toggle while on fire.
    /// </summary>
    public LocId OnFirePopup = "changeling-onfire";

    /// <summary>
    /// Is the ability currently active?
    /// </summary>
    [DataField]
    public bool Active;

    /// <summary>
    /// Should stealth break on an attack?
    /// </summary>
    [DataField]
    public bool RevealOnAttack = true;

    /// <summary>
    /// Should stealth break on taking damage?
    /// </summary>
    [DataField]
    public bool RevealOnDamage = true;

    /// <summary>
    /// How long should you wait before stealth accumulates?
    /// </summary>
    [DataField]
    public TimeSpan WaitTime = TimeSpan.FromSeconds(1);

    /// <summary>
    /// How fast should invisibility recover while active?
    /// </summary>
    [DataField]
    public float VisibilityRate = -1f;

    /// <summary>
    /// Should invisibility break on move?
    /// </summary>
    [DataField]
    public bool BreakOnMove = true;
}
