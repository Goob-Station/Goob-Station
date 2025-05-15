// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
//
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
