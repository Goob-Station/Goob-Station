// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿namespace Content.Server.Power.Components
{
    [RegisterComponent]
    public sealed partial class CableVisComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("node", required:true)]
        public string Node;
    }
}
