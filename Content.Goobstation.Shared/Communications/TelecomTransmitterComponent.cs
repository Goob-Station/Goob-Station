namespace Content.Goobstation.Shared.Communications;

/// <summary>
/// Entities with <see cref="TelecomTransmitterComponent"/> are needed to transmit messages using headsets BETWEEN MAPS.
/// They also need to be powered by <see cref="ApcPowerReceiverComponent"/>.
/// </summary>
[RegisterComponent]
public sealed partial class TelecomTransmitterComponent : Component
{
}
