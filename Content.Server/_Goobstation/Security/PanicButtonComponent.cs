using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Security
{
    [RegisterComponent]
    public sealed partial class PanicButtonComponent : Component
    {
        /// <summary>
        /// What message to send over the radio.
        /// </summary>
        [DataField]
        public LocId DistressMessage = "panic-button-distress";

        /// <summary>
        /// How long is the do-after before the message is sent.
        /// </summary>
        [DataField]
        public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(4);

        /// <summary>
        /// How long is the cooldown before you can send another message.
        /// </summary>
        [DataField]
        public TimeSpan CoolDown = TimeSpan.FromSeconds(45);

        /// <summary>
        /// Which channel to send the message over.
        /// </summary>
        [DataField]
        public ProtoId<RadioChannelPrototype> RadioChannel = "Security";

        /// <summary>
        ///     ID of the cooldown use delay.
        /// </summary>
        [DataField]
        public string DelayId = "button-delay";
    }
}
