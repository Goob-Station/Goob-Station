// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Server.NTR
{
    [RegisterComponent]
    public sealed partial class EventTriggerComponent : Component
    {
        [DataField("eventId", required: true)]
        public string EventId = string.Empty;
    }
}
