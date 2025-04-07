// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 collinlunn <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2020 py01 <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
using Content.Server.Power.NodeGroups;
using Content.Server.Power.Pow3r;

namespace Content.Server.Power.Components
{
    /// <summary>
    ///     Draws power directly from an MV or HV wire it is on top of.
    /// </summary>
    [RegisterComponent]
    public sealed partial class PowerConsumerComponent : BaseNetConnectorComponent<IBasePowerNet>
    {
        /// <summary>
        ///     How much power this needs to be fully powered.
        /// </summary>
        [DataField("drawRate")]
        [ViewVariables(VVAccess.ReadWrite)]
        public float DrawRate { get => NetworkLoad.DesiredPower; set => NetworkLoad.DesiredPower = value; }

        [DataField("showInMonitor")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool ShowInMonitor { get; set; } = true;

        /// <summary>
        ///     How much power this is currently receiving from <see cref="PowerSupplierComponent"/>s.
        /// </summary>
        [ViewVariables]
        public float ReceivedPower => NetworkLoad.ReceivingPower;

        public float LastReceived = float.NaN;

        public PowerState.Load NetworkLoad { get; } = new();

        protected override void AddSelfToNet(IBasePowerNet powerNet)
        {
            powerNet.AddConsumer(this);
        }

        protected override void RemoveSelfFromNet(IBasePowerNet powerNet)
        {
            powerNet.RemoveConsumer(this);
        }
    }
}