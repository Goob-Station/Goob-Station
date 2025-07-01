// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Server.Atmos.Piping.Binary.Components
{
    [RegisterComponent]
    public sealed partial class HeatPumpComponent : Component
    {
        [DataField, ViewVariables(VVAccess.ReadOnly)]
        public bool Active = false;

        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float TransferRate = 100f;

        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float MaxTransferRate = 100f;

        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float TransferCoefficient = 0.01f;

        [DataField]
        public string InletName = "inlet";

        [DataField]
        public string OutletName = "outlet";

        [DataField]
        public SoundSpecifier ValveSound { get; private set; } = new SoundCollectionSpecifier("valveSqueak");
    }
}
