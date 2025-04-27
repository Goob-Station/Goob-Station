// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.PanicButton
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
        /// How long is the cooldown before you can send another message.
        /// </summary>
        [DataField]
        public TimeSpan CoolDown = TimeSpan.FromSeconds(70);

        /// <summary>
        /// Which channel to send the message over.
        /// </summary>
        [DataField]
        public ProtoId<RadioChannelPrototype> RadioChannel = "Security";
    }
}
