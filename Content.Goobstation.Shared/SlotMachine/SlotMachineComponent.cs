using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Audio;
using Content.Shared.Storage;

namespace Content.Goobstation.Shared.SlotMachine;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlotMachineComponent : Component
{
    /// <summary>
    /// Amount of currency required to spin.
    /// </summary>
    [DataField]
    public int SpinCost = 250;

    /// <summary>
    /// List of items that can be selected by emag.
    /// </summary>
    [DataField]
    public List<ProtoId<PrizePrototype>>? EmagPrizes;

    /// <summary>
    /// List of regular prizes.
    /// </summary>
    [DataField(required: true)]
    public List<ProtoId<PrizePrototype>> Prizes;

    /// <summary>
    /// Sound played while spinning.
    /// </summary>
    [DataField]
    public SoundSpecifier SpinSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/SlotMachine/slotmachine_spin.ogg");

    /// <summary>
    /// DoAfter time for while the machine is spinning.
    /// </summary>
    [DataField]
    public float DoAfterTime = 3.8f;

    /// <summary>
    /// Whether the machine is currently spinning.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsSpinning;
}

[Serializable, NetSerializable]
public enum SlotMachineVisuals : byte
{
    Spinning,
}
