using Content.Shared._Goobstation.Clothing.Systems;
using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Clothing.Components;

/// <summary>
///     Component used to designate contol of sealable clothing. It'll contain action to seal clothing.
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SealableClothingSystem))]
public sealed partial class SealableClothingControlComponent : Component
{
    /// <summary>
    ///     Action that used to start sealing
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId SealAction = "ActionClothingSeal";

    [DataField, AutoNetworkedField]
    public EntityUid? SealActionEntity;

    /// <summary>
    ///     Slot required for control to show action
    /// </summary>
    [DataField("requiredSlot"), AutoNetworkedField]
    public SlotFlags RequiredControlSlot = SlotFlags.BACK;

    /// <summary>
    ///     True if clothing in sealing/unsealing process, false if not
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsInProcess = false;

    /// <summary>
    ///     True if clothing is currently sealed and need to start unsealing process. False if opposite.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsCurrentlySealed = false;

    /// <summary>
    ///     Queue of attached parts that should be sealed/unsealed
    /// </summary>
    [DataField, AutoNetworkedField]
    public Queue<NetEntity> ProcessQueue = new();

    /// <summary>
    ///     Uid of entity that currently wear seal control
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? WearerEntity;

    #region Popups & Sounds

    [DataField, AutoNetworkedField]
    public LocId ToggleFailedPopup = "sealable-clothing-equipment-not-toggled";

    [DataField, AutoNetworkedField]
    public LocId SealFailedPopup = "sealable-clothing-equipment-seal-failed";

    [DataField, AutoNetworkedField]
    public SoundSpecifier FailSound = new SoundPathSpecifier("/Audio/Machines/scanbuzz.ogg");

    #endregion
}
