using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.SlotMachine;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlotMachineComponent : Component
{
    [DataField, AutoNetworkedField]
    public int SpinCost = 250;

    public EntProtoId? EmagSpawnEntity;

    [DataField(required: true)]
    public List<ProtoId<PrizePrototype>> Prizes;

    [DataField]
    public SoundSpecifier SpinSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/SlotMachine/slotmachine_spin.ogg");

    [DataField, AutoNetworkedField]
    public float DoAfterTime = 3.8f;

    [DataField, AutoNetworkedField]
    public bool IsSpinning;
}

[Serializable, NetSerializable]
public enum SlotMachineVisuals : byte
{
    Spinning,
}
