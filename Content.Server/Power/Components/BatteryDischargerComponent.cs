// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Content.Server.Power.NodeGroups;

namespace Content.Server.Power.Components
{
    [RegisterComponent]
    public sealed partial class BatteryDischargerComponent : BasePowerNetComponent
    {
        protected override void AddSelfToNet(IPowerNet net)
        {
            net.AddDischarger(this);
        }

        protected override void RemoveSelfFromNet(IPowerNet net)
        {
            net.RemoveDischarger(this);
        }
    }
}
