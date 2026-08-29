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

    #region Sounds

    [DataField]
    public SoundSpecifier SpinSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/SlotMachine/slotmachine_spin.ogg");

    [DataField]
    public SoundSpecifier LoseSound = new SoundPathSpecifier("/Audio/Machines/buzz-two.ogg");

    [DataField]
    public SoundSpecifier SmallWinSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");

    [DataField]
    public SoundSpecifier MediumWinSound = new SoundPathSpecifier("/Audio/Effects/Arcade/win.ogg");

    [DataField]
    public SoundSpecifier BigWinSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/SlotMachine/slotmachine_bigwin.ogg");

    [DataField]
    public SoundSpecifier JackPotWinSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/SlotMachine/slotmachine_jackpotwin.ogg");

    [DataField]
    public SoundSpecifier GodPotWinSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/SlotMachine/slotmachine_godpot.ogg");

    #endregion


    #region DoAfter

    [DataField, AutoNetworkedField]
    public float DoAfterTime = 3.8f;

    [DataField, AutoNetworkedField]
    public bool IsSpinning;

    #endregion
}

[Serializable, NetSerializable]
public enum SlotMachineVisuals : byte
{
    Spinning,
}
