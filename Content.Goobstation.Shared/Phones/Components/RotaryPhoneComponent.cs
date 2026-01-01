using Content.Shared.DeviceLinking;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Phones.Components;

/// <summary>
/// used for the real phones
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RotaryPhoneComponent : Component
{
    /// <summary>
    /// Is the phone connected to another phone or busy
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Engaged;

    /// <summary>
    /// Should the phones transfer
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Connected;

    [DataField, AutoNetworkedField]
    public bool SpeakerPhone;

    /// <summary>
    /// The phone number of this phone
    /// </summary>
    [DataField, AutoNetworkedField]
    public int? PhoneNumber;

    /// <summary>
    /// The phone number the phone is calling
    /// </summary>
    [DataField, AutoNetworkedField]
    public int? DialedNumber;

    /// <summary>
    /// What category under the phone book should this phone be under
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? Category = "Uncategorized";

    /// <summary>
    /// What should the phone be called
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? Name;

    /// <summary>
    /// The connected phone, if any
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ConnectedPhone;

    /// <summary>
    /// What player is holding the other phone
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ConnectedPlayer;

    /// <summary>
    /// What player is holding the other phone
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ConnectedPhoneStand;

    [DataField]
    public ProtoId<SourcePortPrototype> RingPort = "PhoneRingPort";

    [DataField]
    public ProtoId<SourcePortPrototype> OutGoingPort = "PhoneOutgoingPort";

    [DataField]
    public ProtoId<SourcePortPrototype> PickUpPort = "PhonePickupPort";

    [DataField]
    public ProtoId<SourcePortPrototype> HangUpPort = "PhoneHangupPort";

    #region sounds

    [AutoNetworkedField]
    public SpriteSpecifier RopeSprite = new SpriteSpecifier.Rsi(new ResPath("_RMC14/Objects/phone/phone.rsi"), "rope");

    [DataField, AutoNetworkedField]
    public SoundSpecifier SpeakSound = new SoundCollectionSpecifier("RMCPhoneSpeak", AudioParams.Default.WithVolume(-3));

    public SoundSpecifier KeypadPressSound = new SoundPathSpecifier("/Audio/_RMC14/Phone/Tone1.ogg");

    [DataField, AutoNetworkedField]
    public SoundPathSpecifier RingSound = new SoundPathSpecifier("/Audio/_RMC14/Phone/telephone_ring.ogg");

    [DataField, AutoNetworkedField]
    public SoundPathSpecifier RingingSound = new SoundPathSpecifier("/Audio/_RMC14/Phone/ring_outgoing.ogg");

    [DataField, AutoNetworkedField]
    public SoundPathSpecifier HandUpSoundLocal = new SoundPathSpecifier ("/Audio/_RMC14/Phone/phone_busy.ogg");

    [DataField, AutoNetworkedField]
    public SoundPathSpecifier BusySound = new SoundPathSpecifier ("/Audio/_RMC14/Phone/Phone_voicemail.ogg");

    [DataField, AutoNetworkedField]
    public EntityUid? SoundEntity;

    #endregion
}

[Serializable, NetSerializable]
public enum PhoneUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class PhoneKeypadMessage : BoundUserInterfaceMessage
{
    public int Value;

    public PhoneKeypadMessage(int value)
    {
        Value = value;
    }
}

[Serializable, NetSerializable]
public sealed class PhoneBookPressedMessage : BoundUserInterfaceMessage
{
    public int Value;

    public PhoneBookPressedMessage(int value)
    {
        Value = value;
    }
}

[Serializable, NetSerializable]
public sealed class PhoneKeypadClearMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class PhoneDialedMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class GoobPhoneBuiState : BoundUserInterfaceState
{
    public List<PhoneData> Phones { get; }

    public GoobPhoneBuiState(List<PhoneData> phones)
    {
        Phones = phones;
    }
}

[Serializable, NetSerializable]
public sealed class PhoneData
{
    public string Name;
    public string Category;
    public int Number;

    public PhoneData(string name, string category, int number)
    {
        Name = name;
        Category = category;
        Number = number;
    }
}
