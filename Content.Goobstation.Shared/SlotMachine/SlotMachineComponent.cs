using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.SlotMachine;

[RegisterComponent, NetworkedComponent]
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
    public SoundSpecifier SmallWinSound = new SoundPathSpecifier("/Audio/Effect/Cargo/ping.ogg");

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

    [DataField]
    public float SmallWinChance = .20f;

    [DataField]
    public float MediumWinChance = .10f;

    [DataField]
    public float BigWinChance = .05f;

    [DataField]
    public float JackPotWinChance = .01f;

    [DataField]
    public float GodPotWinChance = .001f;

    /// <summary>
    /// Prizes
    /// </summary>

    [DataField]
    public EntProtoId Prize = "spaceCash";

    [DataField]
    public EntProtoId GodPotPrize = "WeaponLauncherHydra";

    /// <summary>
    /// Prize Amounts
    /// </summary>

    [DataField]
    public int SpinCost = 50;

    [DataField]
    public int SmallPrizeAmount = 100;

    [DataField]
    public int MediumPrizeAmount = 150;

    [DataField]
    public int BigPrizeAmount = 1500;

    [DataField]
    public int JackPotPrizeAmount = 10000;

    /// <summary>
    /// DoAfter
    /// </summary>

    [DataField]
    public float DoAfterTime = 3.7f;
}