// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Server.Atmos.Piping.Binary.Components
{
    [RegisterComponent]
    public sealed partial class HeatPumpComponent : Component
    {
        [DataField("active")]
        public bool Active { get; set; } = false;

        [DataField("transferRate")]
        public float TransferRate { get; set; } = 100f;

        [DataField("maxTransferRate")]
        public float MaxTransferRate { get; set; } = 100f;

        [DataField("inlet")]
        public string InletName { get; set; } = "inlet";

        [DataField("outlet")]
        public string OutletName { get; set; } = "outlet";

        [DataField("valveSound")]
        public SoundSpecifier ValveSound { get; private set; } = new SoundCollectionSpecifier("valveSqueak");
    }
}
