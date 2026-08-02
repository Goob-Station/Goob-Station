// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;

namespace Content.Server.Atmos.Components
{
    [RegisterComponent]
    public sealed partial class GasMixtureHolderComponent : Component, IGasMixtureHolder
    {
        [DataField("air")] public GasMixture Air { get; set; } = new GasMixture();
    }
}