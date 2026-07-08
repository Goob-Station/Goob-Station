// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Containers;

namespace Content.Goobstation.Server.NTR
{
    [RegisterComponent]
    public sealed partial class CorporateOverrideComponent : Component
    {
        [DataField]
        public string UnlockedCategory = "NTREvil";

        public ContainerSlot OverrideSlot = default!;
        public const string ContainerId = "CorporateOverrideSlot";
}
}
