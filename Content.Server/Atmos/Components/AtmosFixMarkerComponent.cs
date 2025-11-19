// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Prototypes;

namespace Content.Server.Atmos.Components
{
    /// <summary>
    /// Used by FixGridAtmos. Entities with this may get magically auto-deleted on map initialization in future.
    /// </summary>
    [RegisterComponent, EntityCategory("Mapping")]
    public sealed partial class AtmosFixMarkerComponent : Component
    {
        // See FixGridAtmos for more details
        [DataField("mode")]
        public int Mode { get; set; } = 0;
    }
}
