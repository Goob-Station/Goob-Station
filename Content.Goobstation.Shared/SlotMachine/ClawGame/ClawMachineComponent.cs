using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.SlotMachine.ClawGame;

/// <summary>
/// This is used for the claw game machine.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ClawMachineComponent : Component
{
    [DataField, AutoNetworkedField]
    public float DoAfterTime = 3.9f;

    [DataField]
    public SoundSpecifier PlaySound = new SoundPathSpecifier("/Audio/Machines/Keyboard/keyboard1.ogg");

    [DataField, AutoNetworkedField]
    public bool IsSpinning;

    [DataField(required: true), AutoNetworkedField]
    public List<ProtoId<PrizePrototype>> Prizes;

    [DataField]
    public List<ProtoId<PrizePrototype>> EvilPrizes;
}

[Serializable, NetSerializable]
public enum ClawMachineVisuals : byte
{
    Spinning,
    NormalSprite,
}
