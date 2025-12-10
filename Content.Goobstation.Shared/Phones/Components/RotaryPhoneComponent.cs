using Robust.Shared.GameStates;

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
    public string? Category;

    /// <summary>
    /// The connected phone, if any
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ConnectedPhone;
}
