using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.SlotMachine.CoinFlipper;

/// <summary>
/// This is used for the coinflipper machine.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CoinFliperComponent : Component
{
    [DataField]
    public SoundSpecifier SpinSound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/SlotMachine/slotmachine_spin.ogg");

    [DataField]
    public SoundSpecifier LoseSound = new SoundPathSpecifier("/Audio/Machines/buzz-two.ogg");

    [DataField, AutoNetworkedField]
    public float DoAfterTime = 3.8f;

    [DataField, AutoNetworkedField]
    public bool IsSpinning;

    [DataField, AutoNetworkedField]
    public int PrizeAmount;

    [DataField]
    public SoundSpecifier WinSound = new SoundPathSpecifier("/Audio/Effects/Arcade/win.ogg");
}
