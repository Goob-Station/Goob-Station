using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.SlotMachine;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlotMachineComponent : Component
{
    /// <summary>
    /// Sounds
    /// </summary>

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

    /// <summary>
    /// Chances
    /// </summary>

    [DataField, AutoNetworkedField]
    public float SmallWinChance = .20f;

    [DataField, AutoNetworkedField]
    public float MediumWinChance = .10f;

    [DataField, AutoNetworkedField]
    public float BigWinChance = .05f;

    [DataField, AutoNetworkedField]
    public float JackPotWinChance = .01f;

    [DataField, AutoNetworkedField]
    public float GodPotWinChance = .0001f;

    /// <summary>
    /// Prizes
    /// </summary>

    [DataField, AutoNetworkedField]
    public EntProtoId GodPotPrize = "WeaponShotgunHeavy";

    /// <summary>
    /// Prize Amounts
    /// </summary>

    [DataField, AutoNetworkedField]
    public int SpinCost = 50;

    [DataField, AutoNetworkedField]
    public int SmallPrizeAmount = 100;

    [DataField, AutoNetworkedField]
    public int MediumPrizeAmount = 500;

    [DataField, AutoNetworkedField]
    public int BigPrizeAmount = 1500;

    [DataField, AutoNetworkedField]
    public int JackPotPrizeAmount = 10000;

    /// <summary>
    /// DoAfter
    /// </summary>

    [DataField, AutoNetworkedField]
    public float DoAfterTime = 3.8f;

    [DataField, AutoNetworkedField]
    public bool IsSpinning;
}

[Serializable, NetSerializable]
public enum SlotMachineVisuals : byte
{
    Spinning
}