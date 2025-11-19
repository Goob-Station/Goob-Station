// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

﻿using Content.Shared.Atmos;

namespace Content.Server.Atmos.Components
{
    [RegisterComponent]
    public sealed partial class GasMixtureHolderComponent : Component, IGasMixtureHolder
    {
        [DataField("air")] public GasMixture Air { get; set; } = new GasMixture();
    }
}
