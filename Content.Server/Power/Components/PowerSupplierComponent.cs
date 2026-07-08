// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Power.NodeGroups;
using Content.Server.Power.Pow3r;
using Content.Shared.Guidebook;

namespace Content.Server.Power.Components
{
    [RegisterComponent]
    public sealed partial class PowerSupplierComponent : BaseNetConnectorComponent<IBasePowerNet>
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("supplyRate")]
        [GuidebookData]
        public float MaxSupply { get => NetworkSupply.MaxSupply; set => NetworkSupply.MaxSupply = value; }

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("supplyRampTolerance")]
        public float SupplyRampTolerance
        {
            get => NetworkSupply.SupplyRampTolerance;
            set => NetworkSupply.SupplyRampTolerance = value;
        }

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("supplyRampRate")]
        public float SupplyRampRate
        {
            get => NetworkSupply.SupplyRampRate;
            set => NetworkSupply.SupplyRampRate = value;
        }

        // Goobstation
        [DataField]
        public float SupplyRampScaling // if you want to set this below 1, you're very likely doing something wrong
        {
            get => NetworkSupply.SupplyRampScaling;
            set => NetworkSupply.SupplyRampScaling = value;
        }

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("supplyRampPosition")]
        public float SupplyRampPosition
        {
            get => NetworkSupply.SupplyRampPosition;
            set => NetworkSupply.SupplyRampPosition = value;
        }

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("enabled")]
        public bool Enabled
        {
            get => NetworkSupply.Enabled;
            set => NetworkSupply.Enabled = value;
        }

        [ViewVariables] public float CurrentSupply => NetworkSupply.CurrentSupply;

        [ViewVariables]
        public PowerState.Supply NetworkSupply { get; } = new();

        protected override void AddSelfToNet(IBasePowerNet powerNet)
        {
            powerNet.AddSupplier(this);
        }

        protected override void RemoveSelfFromNet(IBasePowerNet powerNet)
        {
            powerNet.RemoveSupplier(this);
        }
    }
}
