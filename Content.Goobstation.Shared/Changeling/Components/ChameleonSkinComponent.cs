using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChameleonSkinComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionChameleonSkin";

    [DataField, AutoNetworkedField]
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
    [DataField]
    public LocId OnFirePopup = "changeling-onfire";

    /// <summary>
    /// Is the ability currently active?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Active;

    /// <summary>
    /// Should stealth break on an attack?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RevealOnAttack = true;

    /// <summary>
    /// Should stealth break on taking damage?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool RevealOnDamage = true;

    /// <summary>
    /// How long should you wait before stealth accumulates?
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan WaitTime = TimeSpan.FromSeconds(1);

    /// <summary>
    /// How fast should invisibility recover while active?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float VisibilityRate = -1f;

    /// <summary>
    /// Should invisibility break on move?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool BreakOnMove = true;
}
