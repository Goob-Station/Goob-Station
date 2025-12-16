using Robust.Shared.Audio;
using Robust.Shared.GameStates;
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

    [DataField, AutoNetworkedField]
    public EntityUid? ConnectedPlayer;

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
public sealed class PhoneKeypadClearMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class PhoneDialedMessage : BoundUserInterfaceMessage
{
}
