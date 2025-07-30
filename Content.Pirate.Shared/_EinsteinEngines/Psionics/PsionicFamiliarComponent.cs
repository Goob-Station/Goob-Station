using Content.Shared.Popups;
using Robust.Shared.GameStates;

namespace Content.Shared.Abilities.Psionics;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class PsionicFamiliarComponent : Component
{
    /// <summary>
    ///     The entity that summoned this Familiar.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Master;

    /// <summary>
    ///     Whether the familiar is allowed to attack its Master.
    /// </summary>
    [DataField]
    public bool CanAttackMaster;

    /// <summary>
    ///     Popup to play when a familiar that isn't allowed to attack its Master, attempts to do so.
    /// </summary>
    [DataField]
    public string AttackMasterText = "psionic-familiar-cant-attack-master";

    /// <summary>
    ///     Popup type to use when failing to attack the familiar's Master.
    /// </summary>
    [DataField]
    public PopupType AttackPopupType = PopupType.SmallCaution;

    /// <summary>
    ///     Text to display when a Familiar is forced to return from whence it came.
    /// </summary>
    [DataField]
    public string DespawnText = "psionic-familiar-despawn-text";

    /// <summary>
    ///     Popup type to use when a Familiar is forced to return from whence it came.
    /// </summary>
    [DataField]
    public PopupType DespawnPopopType = PopupType.MediumCaution;

    /// <summary>
    ///     Whether a Psionic Familiar is sent back from whence it came if its Master dies.
    /// </summary>
    [DataField]
    public bool DespawnOnMasterDeath = true;

    /// <summary>
    ///     Whether a Psionic Familiar is sent back from whence it came if it dies.
    /// </summary>
    [DataField]
    public bool DespawnOnFamiliarDeath = true;

    /// <summary>
    ///     Whether a Psionic Familiar is sent back from whence it came if its Master is mindbroken.
    /// </summary>
    [DataField]
    public bool DespawnOnMasterMindbroken = true;

    /// <summary>
    ///     Should the Familiar despawn when the player controlling it disconnects.
    /// </summary>
    [DataField]
    public bool DespawnOnPlayerDetach;

    /// <summary>
    ///     Whether a Psionic Familiar inherits its Master's factions.
    ///     This can get people into trouble if the familiar inherits a hostile faction such as Syndicate.
    /// </summary>
    [DataField]
    public bool InheritMasterFactions = true;
}
