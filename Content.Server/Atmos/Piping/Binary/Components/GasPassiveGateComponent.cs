// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Atmos.Piping.Binary.Components
{
    [RegisterComponent]
    public sealed partial class GasPassiveGateComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("inlet")]
        public string InletName { get; set; } = "inlet";

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("outlet")]
        public string OutletName { get; set; } = "outlet";

        [ViewVariables(VVAccess.ReadOnly)]
        [DataField("flowRate")]
        public float FlowRate { get; set; } = 0;
    }
}